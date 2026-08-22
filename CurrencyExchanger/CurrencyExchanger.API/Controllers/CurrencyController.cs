using CurrencyExchanger.DAL.Dao;
using CurrencyExchanger.DAL.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace CurrencyExchanger.API.Controllers;

[ApiController]
[Route("currency/{code?}")]
public class CurrencyController(CurrencyDao currencyDao) : ControllerBase
{
    private readonly CurrencyDao _currencyDao = currencyDao;

    [HttpGet]
    public async Task<ActionResult> GetCurrency(string code)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return BadRequest(new { message = "Код валюты отсутствует в адресе" });
            }

            var currency = await _currencyDao.GetCurrencyByCodeAsync(code);

            if (currency is null)
            {
                return NotFound(new { message = "Валюта не найдена" });
            }

            var currencyDto = CurrencyMapper.ToDto(currency);

            return Ok(currencyDto);
        }
        catch (ArgumentException)
        {
            return BadRequest(new { message = "Код валюты отсутствует в адресе" });
        }
        catch (SqliteException)
        {
            return StatusCode(500, new { message = "База данных недоступна" });
        }
    }
}
