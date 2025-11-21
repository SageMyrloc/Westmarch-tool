using Westmarch_tool.Core.DTOs.Characters.Requests;
using Westmarch_tool.Core.DTOs.Characters.Responses;

namespace Westmarch_tool.Core.Interfaces
{
    public interface ICharacterService
    {
        Task<CharacterResponse?> CreateCharacterAsync(int playerId, CreateCharacterRequest request);
        Task<CharacterResponse?> ImportCharacterAsync(int playerId, ImportCharacterRequest request);
        Task<IEnumerable<CharacterListResponse>> GetPlayerCharactersAsync(int playerId);
        Task<CharacterResponse?> GetCharacterByIdAsync(int characterId, int playerId);
    }
}