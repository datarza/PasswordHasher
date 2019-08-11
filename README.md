Did you like how the [PasswordHasher](https://github.com/aspnet/AspNetCore/blob/master/src/Identity/Extensions.Core/src/PasswordHasher.cs) was implemented in ASP.NET Core? What is the &lt;TUser> used for in class definition? Why only SHA1 and SHA256 are supported? Why the size of salt is a constant? Is the default ASP.NET Core implementation a good choice?

## PasswordHasher in ASP.NET Core could be better implemented

If you are not familiar with the default implementation of PasswordHasher in ASP.NET Core, I would like to recommend an [excellent article](https://andrewlock.net/exploring-the-asp-net-core-identity-passwordhasher) which explains everything.

Some ideas for better implementation of PasswordHasher:

- Setting only the hashing algorithm name should be enough for working with the PasswordHasher
- The salt size and iterations count can be defined as parameters by user or can be counted as default values related to the hashing algorithms
- SHA1 is good for runtime performance, SHA256 is enough for security storage, SHA384 and SHA512 hashing algorithms should be also implemented 
- The result should not contain any hashing parameters, which means: do no not keep salt size, iterations count or hashing algorithm name in the hash
- The result should be the <code>string</code> that can be stored in the database with predictable length
- Internal <code>byte[]</code> comparator for verifing passwords should be a microservice

### Implementation

[![Build Status](https://travis-ci.org/CanadianBeaver/PasswordHasher.svg?branch=master)](https://travis-ci.org/CanadianBeaver/PasswordHasher)

The default password hasher for ASP.NET Core Identity uses <code>PBKDF2</code> for password hashing that is not support all hashing algorithms. The [Rfc2898DeriveBytes class](https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.rfc2898derivebytes?view=netcore-2.2) from the <code>System.Security.Cryptography</code> namespace supports all that we need to get the result we wanted. This class can generate pseudo-randomized salt and supports all SHA hashing algorithms.

#### Method for hashing passwords

```csharp
public string HashPassword(string password)
{
    byte[] saltBuffer;
    byte[] hashBuffer;
    
    using (var keyDerivation = new Rfc2898DeriveBytes(password, options.SaltSize, options.Iterations, options.HashAlgorithmName))
    {
        saltBuffer = keyDerivation.Salt;
        hashBuffer = keyDerivation.GetBytes(options.HashSize);
    }
    
    byte[] result = new byte[options.HashSize + options.SaltSize];
    Buffer.BlockCopy(hashBuffer, 0, result, 0, options.HashSize);
    Buffer.BlockCopy(saltBuffer, 0, result, options.HashSize, options.SaltSize);
    return Convert.ToBase64String(result);
}
```

#### Method for verifing the hash and passwords

```csharp
public bool VerifyHashedPassword(string hashedPassword, string providedPassword)
{
    byte[] hashedPasswordBytes = Convert.FromBase64String(hashedPassword);
    if (hashedPasswordBytes.Length != options.HashSize + options.SaltSize)
    {
            return false;
    }

    byte[] hashBytes = new byte[options.HashSize];
    Buffer.BlockCopy(hashedPasswordBytes, 0, hashBytes, 0, options.HashSize);
    byte[] saltBytes = new byte[options.SaltSize];
    Buffer.BlockCopy(hashedPasswordBytes, options.HashSize, saltBytes, 0, options.SaltSize);

    byte[] providedHashBytes;
    using (var keyDerivation = new Rfc2898DeriveBytes(providedPassword, saltBytes, options.Iterations, options.HashAlgorithmName))
    {
            providedHashBytes = keyDerivation.GetBytes(options.HashSize);
    }

    return comparer.Equals(hashBytes, providedHashBytes);
}
```

### Setting up

The parameters for PasswordHasher can be specified in <code>Startup.cs</code> via [Options pattern](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-2.2). Also, in <code>Startup.cs</code> can be registered the PasswordHasher as a microservice.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Configuring PasswordHasher
    services.Configure<PasswordHasherOptions>(options =>
    {
        options.HashAlgorithm = PasswordHasherAlgorithms.SHA1;
        options.SaltSize = 16;
        options.Iterations = 8192;
    });

    // Registering PasswordHasher
    services.AddPasswordHasher();
    
    services.AddMvc();
}
```

### Using example

```csharp
public class IndexModel : PageModel
{
    private readonly IPasswordHasher hasher;

    public IndexModel(IPasswordHasher hasher)
    {
        this.hasher = hasher;
    }
    
    public void OnGet()
    {
        var password = "my password";
        var passwordHash = hasher.HashPassword(password);
        var passwordCheck = hasher.VerifyHashedPassword(passwordHash, password);
    }
}
```

### Support or Contact

Having questions? [Contact me](https://github.com/CanadianBeaver) and I will help you sort it out.

<style>.inner { min-width: 800px !important; max-width: 70% !important;}</style>
