public class SingleCardChoiceController : CardView
{
    public void OnMouseDown()
    {
        CardChoiceManager.instance.CardClicked(this);
    }
}
