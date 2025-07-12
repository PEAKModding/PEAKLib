using Photon.Pun;

namespace PEAKLib.Items;

public class Action_TestBallRecolor : ItemAction
{
    public override void RunAction()
    {
        var testBall = item.GetComponent<TestBall>();
        if (testBall != null)
        {
            testBall.RandomRecolor();
        }
    }
}
