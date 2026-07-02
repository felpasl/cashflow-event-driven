namespace LancamentosService.Application;

public static class MerchantContext
{
    public const string HeaderName = "X-Merchant-Id";

    public static bool TryGetMerchantId(HttpRequest request, out Guid merchantId)
    {
        merchantId = Guid.Empty;
        return request.Headers.TryGetValue(HeaderName, out var value)
            && Guid.TryParse(value.ToString(), out merchantId)
            && merchantId != Guid.Empty;
    }
}
