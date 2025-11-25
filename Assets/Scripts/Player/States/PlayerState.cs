using Soulslike.Core;
using Soulslike.Player.Controller;

namespace Soulslike.Player.States
{
    public abstract class PlayerState : EntityState
    {
        
        // The player controller
        protected PlayerController Controller { get; }
        
        // Class construction
        protected PlayerState(PlayerController controller)
        {
            
            // Assign the controller variable
            this.Controller = controller;
        }
    }
}