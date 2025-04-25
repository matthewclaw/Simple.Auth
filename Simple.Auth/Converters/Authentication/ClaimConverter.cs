using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Simple.Auth.Converters
{
    public class ClaimConverter : JsonConverter<Claim>
    {
        public override Claim Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected start of object for Claim.");
            }

            string type = null;
            string value = null;
            string valueType = null;
            string issuer = null;
            string originalIssuer = null;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return new Claim(type, value, valueType, issuer, originalIssuer);
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string propertyName = reader.GetString();
                    reader.Read(); // Move to the property value

                    switch (propertyName)
                    {
                        case "Type":
                            type = reader.GetString();
                            break;

                        case "Value":
                            value = reader.GetString();
                            break;

                        case "ValueType":
                            valueType = reader.GetString();
                            break;

                        case "Issuer":
                            issuer = reader.GetString();
                            break;

                        case "OriginalIssuer":
                            originalIssuer = reader.GetString();
                            break;

                        default:
                            // Ignore unknown properties
                            break;
                    }
                }
            }

            throw new JsonException("Unexpected end of JSON stream while reading Claim.");
        }

        public override void Write(Utf8JsonWriter writer, Claim claim, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteString("Type", claim.Type);
            writer.WriteString("Value", claim.Value);
            writer.WriteString("ValueType", claim.ValueType);
            writer.WriteString("Issuer", claim.Issuer);
            writer.WriteString("OriginalIssuer", claim.OriginalIssuer);

            writer.WriteEndObject();
        }
    }
}