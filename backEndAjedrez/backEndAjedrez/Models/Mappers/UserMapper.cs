namespace backEndAjedrez.Models.Mappers;
using backEndAjedrez.Models.Database.Entities;
using backEndAjedrez.Models.Dtos;

public class UserMapper
{

    public UserDto ToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            NickName = user.NickName,
            Email = user.Email,
            Avatar = user.Avatar,
            Status = user.Status            
        };
    }

    public IEnumerable<UserDto> usersToDto(IEnumerable<User> users)
    {
        return users.Select(ToDto);
    }

}

