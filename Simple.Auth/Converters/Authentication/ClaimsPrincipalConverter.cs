using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Simple.Auth.Converters
{
    public class ClaimsPrincipalConverter : JsonConverter<ClaimsPrincipal>
    {
        public override ClaimsPrincipal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected StartObject token.");
            }

            var claims = new List<Claim>();
            ClaimsIdentity identity = null;
            string? authenticationType = null;
            string? nameClaimType = null;
            string? roleClaimType = null;
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    break;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException("Expected PropertyName token.");
                }

                string propertyName = reader.GetString();
                reader.Read(); // Move to property value
                switch (propertyName)
                {
                    case "Claims":
                        if (reader.TokenType == JsonTokenType.StartArray)
                        {
                            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                            {
                                claims.Add(JsonSerializer.Deserialize<Claim>(ref reader, options));
                            }
                        }
                        else
                        {
                            throw new JsonException("Expected StartArray for Claims.");
                        }
                        break;
                    case "AuthenticationType":
                        authenticationType = reader.GetString();
                        break;
                    case "NameClaimType":
                        nameClaimType = reader.GetString();
                        break;
                    case "RoleClaimType":
                        roleClaimType = reader.GetString();
                        break;
                    default:
                        // Ignore unknown properties
                        break;
                }
            }
            identity = new ClaimsIdentity(claims, authenticationType, nameClaimType, roleClaimType);

            return new ClaimsPrincipal(identity);
        }

        public override void Write(Utf8JsonWriter writer, ClaimsPrincipal value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("Claims");
            writer.WriteStartArray();
            foreach (var claim in value.Claims)
            {
                JsonSerializer.Serialize(writer, claim, options);
            }
            writer.WriteEndArray();

            if (value.Identity is ClaimsIdentity identity)
            {
                if (!string.IsNullOrEmpty(identity.AuthenticationType))
                {
                    writer.WriteString("AuthenticationType", identity.AuthenticationType);
                }
                if (!string.IsNullOrEmpty(identity.NameClaimType))
                {
                    writer.WriteString("NameClaimType", identity.NameClaimType);
                }
                if (!string.IsNullOrEmpty(identity.RoleClaimType))
                {
                    writer.WriteString("RoleClaimType", identity.RoleClaimType);
                }
            }

            writer.WriteEndObject();
        }
    }
}
