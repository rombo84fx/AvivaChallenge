using AvivaChallenge.Api.Domain;
using AvivaChallenge.Api.Providers;
using Xunit;

namespace AvivaChallenge.Tests;

public class PagaFacilFeeCalculationTests
{
    private readonly PagaFacilProvider _provider;

    public PagaFacilFeeCalculationTests()
    {
        var httpClient = new HttpClient { BaseAddress = new Uri("https://localhost") };
        _provider = new PagaFacilProvider(httpClient);
    }

    [Fact]
    public void Cash_Returns15Flat()
    {
        Assert.Equal(15, _provider.CalculateFee(100m, PaymentMode.Cash));
        Assert.Equal(15, _provider.CalculateFee(5000m, PaymentMode.Cash));
    }

    [Theory]
    [InlineData(1000, 10)]
    [InlineData(2000, 20)]
    [InlineData(6000, 60)]
    public void CreditCard_Returns1Percent(decimal amount, decimal expectedFee)
    {
        Assert.Equal(expectedFee, _provider.CalculateFee(amount, PaymentMode.CreditCard));
    }

    [Fact]
    public void Transfer_NotSupported_ReturnsMaxValue()
    {
        Assert.Equal(decimal.MaxValue, _provider.CalculateFee(100, PaymentMode.Transfer));
    }

    [Fact]
    public void SupportsCashAndCreditCard()
    {
        Assert.True(_provider.SupportsPaymentMode(PaymentMode.Cash));
        Assert.True(_provider.SupportsPaymentMode(PaymentMode.CreditCard));
        Assert.False(_provider.SupportsPaymentMode(PaymentMode.Transfer));
    }
}

public class CazaPagosFeeCalculationTests
{
    private readonly CazaPagosProvider _provider;

    public CazaPagosFeeCalculationTests()
    {
        var httpClient = new HttpClient { BaseAddress = new Uri("https://localhost") };
        _provider = new CazaPagosProvider(httpClient);
    }

    [Theory]
    [InlineData(1000, 20)]
    [InlineData(1500, 30)]
    [InlineData(2000, 30)]
    [InlineData(5000, 75)]
    [InlineData(6000, 30)]
    [InlineData(10000, 50)]
    public void CreditCard_TieredFees(decimal amount, decimal expectedFee)
    {
        Assert.Equal(expectedFee, _provider.CalculateFee(amount, PaymentMode.CreditCard));
    }

    [Theory]
    [InlineData(100, 5)]
    [InlineData(500, 5)]
    [InlineData(800, 20)]
    [InlineData(1000, 25)]
    [InlineData(2000, 40)]
    public void Transfer_TieredFees(decimal amount, decimal expectedFee)
    {
        Assert.Equal(expectedFee, _provider.CalculateFee(amount, PaymentMode.Transfer));
    }

    [Fact]
    public void Cash_NotSupported_ReturnsMaxValue()
    {
        Assert.Equal(decimal.MaxValue, _provider.CalculateFee(100, PaymentMode.Cash));
    }

    [Fact]
    public void SupportsCreditCardAndTransfer()
    {
        Assert.False(_provider.SupportsPaymentMode(PaymentMode.Cash));
        Assert.True(_provider.SupportsPaymentMode(PaymentMode.CreditCard));
        Assert.True(_provider.SupportsPaymentMode(PaymentMode.Transfer));
    }
}

public class OptimalProviderSelectionTests
{
    [Theory]
    [InlineData(1000, "PagaFacil")]
    [InlineData(3000, "PagaFacil")]
    [InlineData(6000, "CazaPagos")]
    public void CreditCard_SelectsOptimalProvider(decimal amount, string expectedProvider)
    {
        var pagaFacil = new PagaFacilProvider(new HttpClient { BaseAddress = new Uri("https://localhost") });
        var cazaPagos = new CazaPagosProvider(new HttpClient { BaseAddress = new Uri("https://localhost") });
        var selector = new PaymentProviderSelector([pagaFacil, cazaPagos]);

        var result = selector.SelectOptimalProvider(amount, PaymentMode.CreditCard);

        Assert.Equal(expectedProvider, result.Name);
    }
}
