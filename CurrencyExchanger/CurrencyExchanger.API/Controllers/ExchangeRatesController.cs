using CurrencyExchanger.API.Exceptions;
using CurrencyExchanger.API.Models.Requests;
using CurrencyExchanger.API.Models.Responses;
using CurrencyExchanger.BLL.Services;
using CurrencyExchanger.BLL.Validators;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyExchanger.API.Controllers;

[ApiController]
[Route("exchangeRates")]
public class ExchangeRatesController(
    ExchangeRateValidator exchangeRateValidator,
    ExchangeRateService exchangeRateService,
    CurrencyService currencyService
) : ControllerBase
{
    private readonly ExchangeRateValidator _exchangeRateValidator = exchangeRateValidator;
    private readonly ExchangeRateService _exchangeRateService = exchangeRateService;
    private readonly CurrencyService _currencyService = currencyService;

    [HttpGet]
    public async Task<ActionResult> GetExchangesRates()
    {

        var exchangesRates = (await Task.WhenAll((await _exchangeRateService
            .GetAllExchangeRatesAsync())
            .Select(async er =>
            {
                var baseCurrency = await _currencyService.GetCurrencyByIdAsync(er.BaseCurrencyId);
                var targetCurrency = await _currencyService.GetCurrencyByIdAsync(er.TargetCurrencyId);

                if (baseCurrency is null || targetCurrency is null)
                    throw new NotFoundException("Валюта не найдена");

                var exchangeRateResponse = new ExchangeRateResponse
                {
                    Id = er.Id,
                    BaseCurrency = new CurrencyResponse
                    {
                        Id = baseCurrency.Id,
                        Name = baseCurrency.FullName,
                        Code = baseCurrency.Code,
                        Sign = baseCurrency.Sign
                    },
                    TargetCurrency = new CurrencyResponse
                    {
                        Id = targetCurrency.Id,
                        Name = targetCurrency.FullName,
                        Code = targetCurrency.Code,
                        Sign = targetCurrency.Sign
                    },
                    Rate = er.Rate
                };
                return exchangeRateResponse;
            })))
            .ToList();

        return Ok(exchangesRates);
    }

    [HttpPost]
    public async Task<ActionResult<ExchangeRateResponse>> AddExchangeRate([FromForm] ExchangeRateRequest request)
    {
        var baseCurrencyCode = request.BaseCurrencyCode;
        var targetCurrencyCode = request.TargetCurrencyCode;
        var rate = request.Rate;

        if (baseCurrencyCode is null || targetCurrencyCode is null || rate is null)
            throw new BadRequestException("Отсутствует нужное поле формы");

        var baseCurrency = await _currencyService.GetCurrencyByCodeAsync(baseCurrencyCode);
        var targetCurrency = await _currencyService.GetCurrencyByCodeAsync(targetCurrencyCode);

        if (baseCurrency is null || targetCurrency is null)
            throw new NotFoundException("Данной валюты не существует");

        var baseCurrencyId = baseCurrency.Id;
        var targetCurrencyId = targetCurrency.Id;

        if (await _exchangeRateService.IsExchangeRateExistsAsync(baseCurrencyId, targetCurrencyId))
            throw new ConflictException("Такой обменный курс уже существует");

        _exchangeRateValidator.ValidateParameters(baseCurrencyCode, targetCurrencyCode, rate);

        var exchangeRate = await _exchangeRateService.AddExchangeRateAsync(baseCurrencyId, targetCurrencyId, rate);

        if (exchangeRate is null)
            throw new NotFoundException("Данной валюты не существует");

        var exchangeRateResponse = new ExchangeRateResponse
        {
            Id = exchangeRate.Value.ExchangeRate.Id,
            BaseCurrency = new CurrencyResponse
            {
                Id = baseCurrencyId,
                Name = baseCurrency.FullName,
                Code = baseCurrency.Code,
                Sign = baseCurrency.Sign
            },
            TargetCurrency = new CurrencyResponse
            {
                Id = targetCurrencyId,
                Name = targetCurrency.FullName,
                Code = targetCurrency.Code,
                Sign = targetCurrency.Sign
            },
            Rate = rate
        };

        return CreatedAtAction(nameof(GetExchangesRates), exchangeRateResponse);
    }

    [HttpGet("{pair}")]
    public async Task<ActionResult<ExchangeRateResponse>> GetExchangeRate(string pair)
    {
        if (string.IsNullOrWhiteSpace(pair) || pair.Length != 6)
            throw new BadRequestException("Коды валют пары отсутствуют в адресе");

        var exchangeRate = await _exchangeRateService.GetExchangeRateByCodeAsync(pair);

        if (exchangeRate is null)
            throw new NotFoundException("Обменный курс для пары не найден");

        var baseCurrency = exchangeRate.Value.BaseCurrency;
        var targetCurrency = exchangeRate.Value.TargetCurrency;

        var exchangeRateResponse = new ExchangeRateResponse
        {
            Id = exchangeRate.Value.ExchangeRate.Id,
            BaseCurrency = new CurrencyResponse
            {
                Id = baseCurrency.Id,
                Name = baseCurrency.FullName,
                Code = baseCurrency.Code,
                Sign = baseCurrency.Sign
            },
            TargetCurrency = new CurrencyResponse
            {
                Id = targetCurrency.Id,
                Name = targetCurrency.FullName,
                Code = targetCurrency.Code,
                Sign = targetCurrency.Sign,
            },
            Rate = exchangeRate.Value.ExchangeRate.Rate
        };

        return Ok(exchangeRateResponse);
    }

    [HttpPatch("{pair}")]
    public async Task<ActionResult<ExchangeRateResponse>> UpdateExchangeRate([FromRoute] string pair, [FromForm] UpdateExchangeRateRequest request)
    {
        var rate = request.Rate;

        if (string.IsNullOrWhiteSpace(pair) || rate is null || pair.Length != 6)
            throw new BadRequestException("Отсутствует нужное поле формы");

        var baseCurrencyCode = pair.Substring(0, 3);
        var targetCurrencyCode = pair.Substring(3, 3);
        var baseCurrencyId = await _currencyService.GetCurrencyIdByCodeAsync(baseCurrencyCode);
        var targetCurrencyId = await _currencyService.GetCurrencyIdByCodeAsync(targetCurrencyCode);

        if (!(await _exchangeRateService.IsExchangeRateExistsAsync(baseCurrencyId, targetCurrencyId)))
            throw new NotFoundException("Валютная пара отсутствует в базе данных");

        _exchangeRateValidator.ValidateParameters(baseCurrencyCode, targetCurrencyCode, rate);
        var exchangeRate = await _exchangeRateService.UpdateExchangeRateDtoAsync(pair, rate);

        if (exchangeRate is null)
            throw new NotFoundException("Валютная пара отсутствует в базе данных");

        var baseCurrency = exchangeRate.Value.BaseCurrency;
        var targetCurrency = exchangeRate.Value.TargetCurrency;

        var exchangeRateResponse = new ExchangeRateResponse
        {
            Id = exchangeRate.Value.ExchangeRate.Id,
            BaseCurrency = new CurrencyResponse
            {
                Id = baseCurrency.Id,
                Name = baseCurrency.FullName,
                Code = baseCurrency.Code,
                Sign = baseCurrency.Sign
            },
            TargetCurrency = new CurrencyResponse
            {
                Id = targetCurrency.Id,
                Name = targetCurrency.FullName,
                Code = targetCurrency.Code,
                Sign = targetCurrency.Sign
            },
            Rate = exchangeRate.Value.ExchangeRate.Rate
        };

        return Ok(exchangeRateResponse);
    }
}
