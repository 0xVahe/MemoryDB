namespace Engine.Serialization.Binary.Metadata;

internal sealed class HeaderRegistry(
    IEnumerable<IHeaderCodec> codecs,
    IEnumerable<IHeaderValidator> validators)
{
    private readonly Dictionary<int, IHeaderCodec> _codecs = codecs.ToDictionary(c => c.Version);
    private readonly Dictionary<int, IHeaderValidator> _validators = validators.ToDictionary(v => v.Version);

    public bool TryGetCodec(int version, out IHeaderCodec codec) => _codecs.TryGetValue(version, out codec!);
    public bool TryGetValidator(int version, out IHeaderValidator validator) => _validators.TryGetValue(version, out validator!);
}