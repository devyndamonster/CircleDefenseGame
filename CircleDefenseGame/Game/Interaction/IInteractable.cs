namespace CircleDefenseGame.Game.Interaction
{
    public interface IInteractable : IEntity
    {
        public bool IsMouseOver(int mouseX, int mouseY);

        public void MouseDown(GameManager game);

        public void MouseUp(GameManager game);
    }
}
