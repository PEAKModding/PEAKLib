using System;
using PEAKLib.Items;
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
        // test triggered status
        character.refs.afflictions.AddStatus(TestsPlugin.SpikyStatus, 0.1f);

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
