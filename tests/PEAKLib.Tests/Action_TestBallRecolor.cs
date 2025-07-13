using System;
using Photon.Pun;

namespace PEAKLib.Tests;

public class Action_TestBallRecolor : ItemAction
{
    public override void RunAction()
    {
        var testBall = item.GetComponent<TestBall>();
        if (testBall == null)
            throw new NullReferenceException("Component is null!");

        testBall.RandomRecolor();
        if (!photonView.AmController)
        {
            return;
        }
        photonView.RPC(nameof(FinishRecolor), RpcTarget.All);
    }

    [PunRPC]
    private void FinishRecolor()
    {
        var testBall = item.GetComponent<TestBall>();
        if (testBall == null)
            throw new NullReferenceException("Component is null!");

        testBall.OnInstanceDataSet();
    }
}
