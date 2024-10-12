namespace LactoseWebApp.Repo;

public interface IBasicKeyValueRepo<TModel> where TModel : class
{
    public Task<ISet<string>> Query();
    public Task<ICollection<TModel>> Get(ICollection<string> ids);
    public Task<TModel?> Set(TModel model);
    public Task<ICollection<string>> Delete(ICollection<string> ids);
    public Task<bool> Clear();

    public async Task<TModel?> Get(string id)
    {
        // Default implementation; override if necessary.

        ICollection<TModel> models = await Get([id]);
        return models.FirstOrDefault();
    }

    public async Task<bool> Delete(string id)
    {
        // Default implementation; override if necessary.

        ICollection<string> deleted = await Delete([id]);
        return !deleted.IsEmpty();
    }
}