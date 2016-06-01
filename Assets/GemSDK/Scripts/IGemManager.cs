using System;

namespace GemSDK.Unity
{
    public interface IGemManager
    {
        /// <summary>
        /// Connects to Gem Service
        /// </summary>
        void Connect();
        
        /// <summary>
        /// Disconnects from Gem Service
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Returns gem instance by its address. Initiates connecting process if GemManager is connected
        /// </summary>
        /// <param name="address">Gem address string, for example, "BA:FA:56:FC:DC:14"</param>
        /// <returns></returns>
        IGem GetGem(String address);

        /// <summary>
        /// Returns gem instance by the position in the list of paired gems. Initiates connecting process if GemManager is connected
        /// </summary>
        /// <param name="pos">Position from the start in the list of paired gems. On Windows the list is degined by settings.
        /// On Android - by Gem SDK Utility app. Not recommended in production code (use your own address saving logic).
        /// </param>
        /// <returns>Gem instance</returns>
        IGem GetGem(int pos);
    }
}
