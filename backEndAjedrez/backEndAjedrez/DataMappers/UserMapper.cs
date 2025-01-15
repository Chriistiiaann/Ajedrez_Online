namespace backEndAjedrez.DataMappers;
using backEndAjedrez.DTOs;
using backEndAjedrez.Models;


    public class UserMapper
    {

        public UserDto ToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                NickName = user.NickName,
                Email = user.Email,
            };
        }

        public IEnumerable<UserDto> usersToDto(IEnumerable<User> users)
        {
            return users.Select(ToDto);
        }

}

