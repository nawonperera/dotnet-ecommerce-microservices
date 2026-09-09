using System;
using System.Collections.Generic;
using System.Text;

namespace eCommerce.Core.DTO;

public record AuthenticationResponse(Guid UserID, string? Email, string? PersonName, string? Gender, string? Token, bool Success)
{
    public AuthenticationResponse() : this(default, default, default, default, default, default) // The secondary parameterless constructor calls the primary constructor using "this". The default Guid value is 00000000-0000-0000-0000-000000000000, and the default string value is null. When we create a new instance using the "new" keyword, the parameterized (primary) constructor is used. However, tools like AutoMapper rely on the parameterless constructor.
    {
    }
};

