using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PEAKLib.ModConfig;

internal sealed class InputBindingCaptureService : MonoBehaviour
{
    private enum CaptureKind
    {
        None,
        KeyCode,
        Path,
    }

    private static readonly KeyCode[] KeyCodes = (KeyCode[])Enum.GetValues(typeof(KeyCode));

    private readonly InputAction dummyAction = CreateDummyAction();
    private InputActionRebindingExtensions.RebindingOperation? operation;
    private InputActionRebindingExtensions.RebindingOperation? operationToDispose;
    private InputAction? pauseAction;
    private UnityEngine.Object? owner;
    private CaptureKind captureKind;
    private Action<KeyCode>? keyCodeCompleted;
    private Action<string>? pathCompleted;
    private Action? canceled;
    private Action? pendingNotification;
    private bool isDestroying;

    public bool IsCapturing =>
        captureKind != CaptureKind.None
        || operationToDispose != null
        || pendingNotification != null;

    public bool TryCaptureKeyCode(
        UnityEngine.Object captureOwner,
        Action<KeyCode> onCompleted,
        Action onCanceled
    )
    {
        if (onCompleted == null || !TryBegin(captureOwner, CaptureKind.KeyCode, onCanceled))
            return false;

        keyCodeCompleted = onCompleted;

        try
        {
            dummyAction.RemoveAllBindingOverrides();
            operation = dummyAction
                .PerformInteractiveRebinding(0)
                .OnPotentialMatch(CancelIfPauseControl);
            operation.Start();
            return true;
        }
        catch (Exception exception)
        {
            ModConfigPlugin.Log.LogError(exception);
            ResetFailedStart();
            return false;
        }
    }

    public bool TryCapturePath(
        UnityEngine.Object captureOwner,
        Action<string> onCompleted,
        Action onCanceled
    )
    {
        if (onCompleted == null || !TryBegin(captureOwner, CaptureKind.Path, onCanceled))
            return false;

        pathCompleted = onCompleted;
        string? capturedPath = null;

        try
        {
            dummyAction.RemoveAllBindingOverrides();
            operation = dummyAction
                .PerformInteractiveRebinding(0)
                .WithCancelingThrough("<Keyboard>/escape")
                .OnApplyBinding((_, path) => capturedPath = path)
                .OnPotentialMatch(CancelIfPauseControl)
                .OnComplete(currentOperation => QueuePathCompleted(currentOperation, capturedPath))
                .OnCancel(QueueCanceled);
            operation.Start();
            return true;
        }
        catch (Exception exception)
        {
            ModConfigPlugin.Log.LogError(exception);
            ResetFailedStart();
            return false;
        }
    }

    public void Cancel(UnityEngine.Object captureOwner)
    {
        if (captureKind == CaptureKind.None || !ReferenceEquals(owner, captureOwner))
            return;

        CancelCurrent();
    }

    private void Update()
    {
        if (captureKind == CaptureKind.None)
            return;

        if (
            Input.GetKeyDown(KeyCode.Escape)
            || (pauseAction != null && pauseAction.WasPressedThisFrame())
        )
        {
            CancelCurrent();
            return;
        }

        if (captureKind != CaptureKind.KeyCode)
            return;

        foreach (KeyCode keyCode in KeyCodes)
        {
            if (Input.GetKeyDown(keyCode))
            {
                CompleteKeyCode(keyCode);
                return;
            }
        }
    }

    private void LateUpdate() => FlushPending();

    private void OnDestroy()
    {
        isDestroying = true;
        CancelCurrent();
        FlushPending();
        dummyAction.Dispose();
    }

    private bool TryBegin(UnityEngine.Object captureOwner, CaptureKind kind, Action onCanceled)
    {
        if (isDestroying || captureOwner == null || onCanceled == null || IsCapturing)
            return false;

        owner = captureOwner;
        captureKind = kind;
        canceled = onCanceled;
        pauseAction = InputSystem.actions?.FindAction("Pause");
        return true;
    }

    private void CompleteKeyCode(KeyCode keyCode)
    {
        InputActionRebindingExtensions.RebindingOperation? currentOperation = operation;
        if (currentOperation?.started == true)
            currentOperation.Cancel();

        Action<KeyCode>? onCompleted = keyCodeCompleted;
        QueueTerminal(currentOperation, () => onCompleted?.Invoke(keyCode));
    }

    private void CancelIfPauseControl(
        InputActionRebindingExtensions.RebindingOperation currentOperation
    )
    {
        if (pauseAction == null || currentOperation.selectedControl == null)
            return;

        foreach (InputBinding binding in pauseAction.bindings)
        {
            if (
                !string.IsNullOrEmpty(binding.effectivePath)
                && InputControlPath.Matches(binding.effectivePath, currentOperation.selectedControl)
            )
            {
                CancelCurrent();
                return;
            }
        }
    }

    private void QueuePathCompleted(
        InputActionRebindingExtensions.RebindingOperation currentOperation,
        string? path
    )
    {
        Action<string>? onCompleted = pathCompleted;
        if (string.IsNullOrEmpty(path))
        {
            QueueCanceled(currentOperation);
            return;
        }

        QueueTerminal(currentOperation, () => onCompleted?.Invoke(path));
    }

    private void QueueCanceled(InputActionRebindingExtensions.RebindingOperation? currentOperation)
    {
        Action? onCanceled = canceled;
        QueueTerminal(currentOperation, () => onCanceled?.Invoke());
    }

    private void QueueTerminal(
        InputActionRebindingExtensions.RebindingOperation? currentOperation,
        Action notification
    )
    {
        if (captureKind == CaptureKind.None || currentOperation != operation)
            return;

        UnityEngine.Object? captureOwner = owner;
        operation = null;
        operationToDispose = currentOperation;
        pauseAction = null;
        owner = null;
        captureKind = CaptureKind.None;
        keyCodeCompleted = null;
        pathCompleted = null;
        canceled = null;
        pendingNotification = () =>
        {
            if (captureOwner != null)
                notification();
        };
    }

    private void CancelCurrent()
    {
        if (captureKind == CaptureKind.None)
            return;

        InputActionRebindingExtensions.RebindingOperation? currentOperation = operation;
        if (captureKind == CaptureKind.Path && currentOperation?.started == true)
        {
            currentOperation.Cancel();
            return;
        }

        if (currentOperation?.started == true)
            currentOperation.Cancel();

        QueueCanceled(currentOperation);
    }

    private void ResetFailedStart()
    {
        InputActionRebindingExtensions.RebindingOperation? failedOperation = operation;
        if (failedOperation?.started == true)
            failedOperation.Cancel();

        operationToDispose = null;
        pendingNotification = null;
        failedOperation?.Dispose();

        operation = null;
        pauseAction = null;
        owner = null;
        captureKind = CaptureKind.None;
        keyCodeCompleted = null;
        pathCompleted = null;
        canceled = null;
    }

    private void FlushPending()
    {
        InputActionRebindingExtensions.RebindingOperation? completedOperation = operationToDispose;
        Action? notification = pendingNotification;
        operationToDispose = null;
        pendingNotification = null;

        completedOperation?.Dispose();

        try
        {
            notification?.Invoke();
        }
        catch (Exception exception)
        {
            ModConfigPlugin.Log.LogError(exception);
        }
    }

    private static InputAction CreateDummyAction()
    {
        var action = new InputAction("ModConfig Input Binding Capture");
        action.AddBinding("<Keyboard>/anyKey");
        return action;
    }
}
