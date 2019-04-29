namespace Shelter.Models
{
    public interface IModelRenderer<TModel, TView>
        where TView : IModelView<TModel>, new()
    {
        TModel FromView(TView view);

        TView ToView(TModel model);
    }
}