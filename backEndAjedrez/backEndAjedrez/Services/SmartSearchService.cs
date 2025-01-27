using backEndAjedrez.Models.Database;
using backEndAjedrez.Models.Database.Entities;
using backEndAjedrez.Models.Dtos;
using backEndAjedrez.Models.Mappers;
using F23.StringSimilarity;
using F23.StringSimilarity.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;

namespace backEndAjedrez.Services;

public class SmartSearchService
{
    private const double THRESHOLD = 0.75;
    private readonly INormalizedStringSimilarity _stringSimilarityComparer;
    private readonly DataContext _dbContext;
    private readonly UserMapper _userMapper;

    public SmartSearchService(DataContext dbContext, UserMapper userMapper)
    {
        _dbContext = dbContext;
        _stringSimilarityComparer = new JaroWinkler();
        _userMapper = userMapper;
    }

    public IEnumerable<UserDto> Search(string query)
    {
        IEnumerable<UserDto> result;

        if (string.IsNullOrWhiteSpace(query))
        {
             result = _userMapper.usersToDto(_dbContext.Users.ToList());
        }
        else
        {
            string[] queryKeys = GetKeys(ClearText(query));
            List<UserDto> matches = new List<UserDto>();

            var users = _dbContext.Users.ToList();

            foreach (var user in users)
            {
                string[] itemKeys = GetKeys(ClearText(user.NickName));

                if (IsMatch(queryKeys, itemKeys))
                {
                    matches.Add(_userMapper.ToDto(user));
                }
            }

            result = matches;
        }

        return result;
    }
    public async Task<IEnumerable<UserDto>> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            var users = await _dbContext.Users.ToListAsync();
            return _userMapper.usersToDto(users);
        }
        else
        {
            string[] queryKeys = GetKeys(ClearText(query));
            var matches = new List<UserDto>();

            var users = await _dbContext.Users.ToListAsync(); 

            foreach (var user in users)
            {
                string[] itemKeys = GetKeys(ClearText(user.NickName));

                if (IsMatch(queryKeys, itemKeys))
                {
                    matches.Add(_userMapper.ToDto(user));
                }
            }

            return matches;
        }
    }
    private bool IsMatch(string[] queryKeys, string[] itemKeys)
    {
        bool isMatch = false;

        for (int i = 0; !isMatch && i < itemKeys.Length; i++)
        {
            string itemKey = itemKeys[i];

            for (int j = 0; !isMatch && j < queryKeys.Length; j++)
            {
                string queryKey = queryKeys[j];

                isMatch = IsMatch(itemKey, queryKey);
            }
        }

        return isMatch;
    }

    // Hay coincidencia si las palabras son las mismas o si item contiene query o si son similares
    private bool IsMatch(string itemKey, string queryKey)
    {
        return itemKey == queryKey
            || itemKey.Contains(queryKey)
            || _stringSimilarityComparer.Similarity(itemKey, queryKey) >= THRESHOLD;
    }

    // Separa las palabras quitando los espacios
    private string[] GetKeys(string query)
    {
        return query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    // Normaliza el texto 
    private string ClearText(string text)
    {
        return RemoveDiacritics(text.ToLower());
    }

    // Quita las tildes a un texto
    private string RemoveDiacritics(string text)
    {
        string normalizedString = text.Normalize(NormalizationForm.FormD);
        StringBuilder stringBuilder = new StringBuilder(normalizedString.Length);

        for (int i = 0; i < normalizedString.Length; i++)
        {
            char c = normalizedString[i];
            UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }
}
