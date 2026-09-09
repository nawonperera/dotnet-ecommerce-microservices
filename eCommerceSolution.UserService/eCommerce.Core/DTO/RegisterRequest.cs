using System;
using System.Collections.Generic;
using System.Text;

namespace eCommerce.Core.DTO;

public record RegisterRequest(string? Email, string? Password, string? PersonName, GenderOptions Gender) 
{
    public RegisterRequest(): this(default, default, default, default)
    {
    }
};

