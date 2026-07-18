using CircleDefenseGame.Game.Rendering;

namespace CircleDefenseGame.Game.Interaction
{
    public class Coin : IEntity, IInteractable, IRenderable
    {
        public required int X { get; set; }

        public required int Y { get; set; }

        public void MouseDown()
        {
            throw new NotImplementedException();
        }

        public void MouseUp()
        {
            throw new NotImplementedException();
        }

        public void Render()
        {
            throw new NotImplementedException();
        }
    }
}
