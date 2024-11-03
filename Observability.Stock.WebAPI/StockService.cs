using Observability.Common.Shared.Dtos;
using TS.Result;

namespace Observability.Stock.WebAPI;

public class StockService(
    HttpClient httpClient)
{
    private static Dictionary<int, int> GetProductStokList()
    {
        Dictionary<int, int> list = new();
        list.Add(1, 10);
        list.Add(2, 20);
        list.Add(3, 30);

        return list;
    }

    public async Task<Result<string>> CheckAndPaymentProcessAsync(StockCheckAndPaymentProcessRequestDto request)
    {
        await Task.CompletedTask;

        var productStokList = GetProductStokList();
        var stokStatus = new List<(int productId, bool hasStockExist)>();
        var stokStatus2 = new List<Tuple<int, bool>>();

        foreach (var item in request.OrderItems)
        {
            var hasExistStok = productStokList.Any(s => s.Key == item.ProductId && s.Value >= item.Count);

            stokStatus.Add((item.ProductId, hasExistStok));
        }

        if (stokStatus.Any(p => p.hasStockExist == false))
        {
            return Result<string>.Failure(400, "Stok yetersiz");
        }

        CreatePaymentDto createPaymentDto = new(request.OrderCode, request.OrderItems.Sum(s => s.Count * s.Price));

        var message = await httpClient.PostAsJsonAsync("https://localhost:7123/api/pay", createPaymentDto);

        if (!message.IsSuccessStatusCode)
        {
            var content = await message.Content.ReadFromJsonAsync<Result<string>>();
            return content!;
        }

        return "Stok rezerve edildi";
    }
}
