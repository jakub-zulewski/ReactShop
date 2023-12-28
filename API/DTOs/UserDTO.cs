namespace API.DTOs;

public class UserDTO
{
    public string Email { get; set; }
    public string Token { get; set; }
    public BasketDTO Basket { get; set; }
}
