using Observability.Common.Shared.Dtos;
using TS.Result;

namespace Observability.Orders.WebAPI;

public sealed class StokService(HttpClient httpClient)
{
    public async Task<Result<string>> CheckStockAndStartPaymentAsync(StockCheckAndPaymentProcessRequestDto request, CancellationToken cancellationToken)
    {
        var responseMessage = await httpClient.PostAsJsonAsync("https://localhost:7126/api/CheckAndPaymentStart", request);

        var response = await responseMessage.Content.ReadFromJsonAsync<Result<string>>();

        return response!;
    }
}
