using AvivaChallenge.Api.Domain;

namespace AvivaChallenge.Api.Providers;

public interface IPaymentProviderSelector
{
    IPaymentProvider SelectOptimalProvider(decimal amount, PaymentMode mode);
}

public class PaymentProviderSelector : IPaymentProviderSelector
{
    private readonly IEnumerable<IPaymentProvider> _providers;

    public PaymentProviderSelector(IEnumerable<IPaymentProvider> providers)
    {
        _providers = providers;
    }

    public IPaymentProvider SelectOptimalProvider(decimal amount, PaymentMode mode)
    {
        var eligible = _providers.Where(p => p.SupportsPaymentMode(mode)).ToList();

        if (eligible.Count == 0)
            throw new InvalidOperationException($"No payment provider supports payment mode '{mode}'.");

        return eligible
            .OrderBy(p => p.CalculateFee(amount, mode))
            .First();
    }
}
