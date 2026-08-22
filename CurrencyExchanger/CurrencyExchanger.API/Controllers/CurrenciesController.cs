using CurrencyExchanger.BLL.Validators;
using CurrencyExchanger.DAL.Dao;
using CurrencyExchanger.DAL.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace CurrencyExchanger.API.Controllers;

[ApiController]
[Route("currencies")]
public class CurrenciesController(CurrencyValidator currencyValidator, CurrencyDao currencyDao) : ControllerBase
{
    private readonly CurrencyValidator _currencyValidator = currencyValidator;
    private readonly CurrencyDao _currencyDao = currencyDao;

    [HttpGet]
    public async Task<ActionResult> GetCurrencies()
    {
        try
        {
            var currencies = (await _currencyDao.GetAllCurrenciesAsync())
                .Select(CurrencyMapper.ToDto)
                .ToList();

            return Ok(currencies);
        }
        catch (SqliteException)
        {
            return StatusCode(500, new { message = "База данных недоступна" });
        }
    }

    [HttpPost]
    public async Task<ActionResult> AddCurrency([FromForm] string code, [FromForm] string name, [FromForm] string sign)
    {
        try
        {
            if (code is null || name is null || sign is null)
            {
                return BadRequest(new { message = "Отсутствует нужное поле формы" });
            }

            if (await _currencyDao.IsCurrencyExistsAsync(code))
            {
                return Conflict(new { message = "Валюта с таким кодом уже существует в базе данных" });
            }

            _currencyValidator.ValidateCurrencyParameters(code, name, sign);
            var currency = await _currencyDao.AddCurrencyAsync(name, code, sign);
            var currencyDto = CurrencyMapper.ToDto(currency);

            return CreatedAtAction(nameof(GetCurrencies), new { code = currencyDto.Code }, currencyDto);
        }
        catch (ArgumentException)
        {
            return BadRequest(new { message = "Отсутствует нужное поле формы" });
        }
        catch (SqliteException)
        {
            return StatusCode(500, new { message = "База данных недоступна" });
        }
    }
}
