using Android.OS;

public class SpyToolServiceBinder : Binder
{
    private SpyToolService _service;

    public SpyToolServiceBinder(SpyToolService service)
    {
        _service = service;
    }

    public SpyToolService GetSpyToolService()
    {
        return _service;
    }
}