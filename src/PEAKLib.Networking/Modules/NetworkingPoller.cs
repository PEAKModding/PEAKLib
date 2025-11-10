using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PEAKLib.Networking.Modules
{
    internal class NetworkingPoller : MonoBehaviour
    {
        void Update()
        {
            try
            {
                NetworkingPlugin.Service?.PollReceive();
            }
            catch (System.Exception ex)
            {
                NetworkingPlugin.Log.LogError($"NetworkingPoller exception: {ex}");
            }
        }
    }
}
