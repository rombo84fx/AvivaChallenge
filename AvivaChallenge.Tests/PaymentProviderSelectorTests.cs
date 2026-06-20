using AvivaChallenge.Api.Domain;
using AvivaChallenge.Api.Providers;
using NSubstitute;
using Xunit;

namespace AvivaChallenge.Tests;

public class PaymentProviderSelectorTests
{
    private readonly IPaymentProvider _pagaFacil;
    private readonly IPaymentProvider _cazaPagos;
    private readonly PaymentProviderSelector _selector;

    public PaymentProviderSelectorTests()
    {
        _pagaFacil = Substitute.For<IPaymentProvider>();
        _pagaFacil.Name.Returns("PagaFacil");

        _cazaPagos = Substitute.For<IPaymentProvider>();
        _cazaPagos.Name.Returns("CazaPagos");

        _selector = new PaymentProviderSelector([_pagaFacil, _cazaPagos]);
    }

    [Fact]
    public void SelectOptimalProvider_ChoosesProviderWithLowestFee()
    {
        _pagaFacil.SupportsPaymentMode(PaymentMode.CreditCard).Returns(true);
        _pagaFacil.CalculateFee(1000m, PaymentMode.CreditCard).Returns(10m);

        _cazaPagos.SupportsPaymentMode(PaymentMode.CreditCard).Returns(true);
        _cazaPagos.CalculateFee(1000m, PaymentMode.CreditCard).Returns(20m);

        var result = _selector.SelectOptimalProvider(1000m, PaymentMode.CreditCard);

        Assert.Equal("PagaFacil", result.Name);
    }

    [Fact]
    public void SelectOptimalProvider_OnlyCashSupported_ReturnsPagaFacil()
    {
        _pagaFacil.SupportsPaymentMode(PaymentMode.Cash).Returns(true);
        _pagaFacil.CalculateFee(500m, PaymentMode.Cash).Returns(15m);

        _cazaPagos.SupportsPaymentMode(PaymentMode.Cash).Returns(false);

        var result = _selector.SelectOptimalProvider(500m, PaymentMode.Cash);

        Assert.Equal("PagaFacil", result.Name);
    }

    [Fact]
    public void SelectOptimalProvider_OnlyTransferSupported_ReturnsCazaPagos()
    {
        _pagaFacil.SupportsPaymentMode(PaymentMode.Transfer).Returns(false);

        _cazaPagos.SupportsPaymentMode(PaymentMode.Transfer).Returns(true);
        _cazaPagos.CalculateFee(800m, PaymentMode.Transfer).Returns(20m);

        var result = _selector.SelectOptimalProvider(800m, PaymentMode.Transfer);

        Assert.Equal("CazaPagos", result.Name);
    }

    [Fact]
    public void SelectOptimalProvider_NoProviderSupportsMode_ThrowsInvalidOperationException()
    {
        _pagaFacil.SupportsPaymentMode(PaymentMode.Transfer).Returns(false);
        _cazaPagos.SupportsPaymentMode(PaymentMode.Transfer).Returns(false);

        Assert.Throws<InvalidOperationException>(() =>
            _selector.SelectOptimalProvider(100m, PaymentMode.Transfer));
    }
}
