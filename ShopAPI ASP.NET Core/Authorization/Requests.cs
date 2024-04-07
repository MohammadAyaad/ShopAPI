using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Threading.Tasks;

using ShopAPI.Database;
using ShopAPI.Http;
using ShopAPI.Authorization.Permissions_Roles;
using ShopAPI.Requests;
using ShopAPI.Dashboard.Api.Logs;

using static ShopAPI.Http.HttpStatusMap;
using static ShopAPI.Utilities.Functions;
using static ShopAPI.Constants.KeywordsConstants;
using static ShopAPI.Http.HeadersConstants;

namespace ShopAPI.Requests
{
    //[Respondable]
    public static partial class Requests
    {
        public const string createAccountRegex = /* language=regex */@"\A\/api\/accounts\/create(\/)?(\?((&)?[a-zA-Z]+\=+[a-zA-Z0-9]+(;)?)*)?$";
        [RequestHandler(HttpMethods.POST, createAccountRegex, /*Permissions.CREATE_ACCOUNT*/ Permissions.RESOURCE_OPEN)]
        public static async Task<Response> CreateAccount(RequestInputParamsWrapper input_params)
        {
            (bool valid, JsonNode requestBody) = Utilities.Functions.IsValidJson_Parse(input_params.body);
            if (!valid)
                return new ErrorResponse(S_BadRequest, "Invalid body data");

            if (!Utilities.Functions.IsValidJsonStructure(requestBody, JsonObject.Parse($"{{\"{KW_EMAIL}\":\"\",\"{KW_PASSWORD}\":\"\",\"{KW_USERNAME}\":\"\",\"{KW_LASTNAME}\":\"\",\"{KW_BIRTH_DATE}\":\"\"}}")))
                return new ErrorResponse(S_BadRequest, "Invalid body data");
            JsonObject o = new JsonObject();

            (bool success, string msg_info, input_params.token) = Queries.CreateAccount(
            Program.connection,
            requestBody[KW_EMAIL].ToString(),
            requestBody[KW_PASSWORD].ToString(),
            requestBody[KW_USERNAME].ToString(),
            requestBody[KW_LASTNAME].ToString(),
            DateOnly.Parse(requestBody[KW_BIRTH_DATE].ToString()));
            if (success)
            {
                o.Add(KW_INFO, msg_info);
                o.Add(KW_TOKEN, input_params.token);
                return new DataResponse(S_OK, DataFormat.Json, o.ToString());
            }
            else
            {
                return new ErrorResponse(S_BadRequest, msg_info);
            }
        }

        public const string accountLoginRegex = /* language=regex */@"\A\/api\/accounts\/login(\/)?(\?((&)?[a-zA-Z]+\=+[a-zA-Z0-9]+(;)?)*)?$";
        [RequestHandler(HttpMethods.POST, accountLoginRegex)]
        public static async Task<Response> LoginAccount(RequestInputParamsWrapper input_params)
        {
            JsonObject requestBody = JsonObject.Parse(input_params.body).AsObject();

            if (requestBody.ContainsKey(KW_EMAIL) &&
                requestBody.ContainsKey(KW_PASSWORD))
            {
                if (!((requestBody[KW_EMAIL].GetValue<JsonElement>().ValueKind == JsonValueKind.String) &&
                    (requestBody[KW_PASSWORD].GetValue<JsonElement>().ValueKind == JsonValueKind.String)))
                    return new ErrorResponse(S_BadRequest, "Bad request body");

                bool success = false; string msg_info = null; input_params.token = null;

                if (requestBody.ContainsKey(KW_PERMISSIONS))
                {
                    if ((requestBody[KW_PERMISSIONS].GetValue<JsonElement>().ValueKind == JsonValueKind.String) && (UserRoles.PermissionTable.ContainsKey(requestBody[KW_PERMISSIONS].ToString())))
                    {
                        (success, msg_info, input_params.token) = Queries.Login(
                            Program.connection,
                            requestBody[KW_EMAIL].ToString(),
                            requestBody[KW_PASSWORD].ToString(),
                            ((Permissions)UserRoles.PermissionTable[requestBody[KW_PERMISSIONS].ToString()]));
                    }
                    else
                    {
                        return new ErrorResponse(S_Unauthorized, msg_info);
                    }
                }
                else
                {
                    (success, msg_info, input_params.token) = Queries.Login(
                    Program.connection,
                    requestBody[KW_EMAIL].ToString(),
                    requestBody[KW_PASSWORD].ToString(),
                    (Permissions)UserRoles.PERMISSIONS_DefaultUserAccount);
                }

                if (success)
                {
                    JsonObject o = new JsonObject();
                    o.Add("info", msg_info);
                    o.Add(KW_TOKEN, input_params.token);
                    return new DataResponse(S_OK, DataFormat.Json, o.ToString());
                }
                else
                {
                    return new ErrorResponse(S_BadRequest, msg_info);
                }
            }
            else
            {

                return new ErrorResponse(S_BadRequest, "Invalid query parameters");
            }
        }

        public const string accountLogoutRegex = /* language=regex */@"\A\/api\/accounts\/logout(\/)?(\?((&)?[a-zA-Z]+\=+[a-zA-Z0-9]+(;)?)*)?$";
        [RequestHandler(HttpMethods.POST, accountLogoutRegex)]
        public static async Task<Response> LogoutAccount(RequestInputParamsWrapper input_params)
        {
            if (input_params.request.Headers.AllKeys.Contains(HN_Authorization))
            {
                string authorization = input_params.request.Headers[HN_Authorization];
                string[] auth_parts = authorization.Split(' ');
                if (auth_parts.Length != 2)
                {
                    return new ErrorResponse(S_Unauthorized, "Bad Authorization header");
                }
                else
                {
                    if (auth_parts[0] != "Bearer")
                        return new ErrorResponse(S_Unauthorized, "Bad Authorization header");


                    try
                    {
                        Queries.Logout(Program.connection, auth_parts[1]);
                        return new DataResponse(S_OK, DataFormat.Text, "Logged out");
                    }
                    catch (Exception e)
                    {
                        ExceptionsLog.ReportException(Program.connection, e,5, 100, Guid.NewGuid());
                        //the real reason of the exception is not exposed due to the error nature being unidentifiable (is it good to expose to the user => maybe not) SECURITY_REASON
                        //TODO: improve error handling
                        //BUG: if a bad Encrypted-JWT token was provided its going to throw a "Internal Server Error" instead of "Unauthorized" due to a bad token, 
                        return new ErrorResponse(S_InternalServerError, S_InternalServerError.status);
                    }
                }
            }
            else
            {
                return new ErrorResponse(S_Unauthorized, "No Authorization header provided");
            }

        }

        public const string accountDeleteRegex = /* language=regex */@"\A\/api\/accounts\/delete(\/)?(\?((&)?[a-zA-Z]+\=+[a-zA-Z0-9]+(;)?)*)?$";
        [RequestHandler(HttpMethods.DELETE, accountDeleteRegex, Permissions.DELETE_ACCOUNT)]
        public static async Task<Response> DeleteAccount(RequestInputParamsWrapper input_params)
        {
            //should execute only when the user is allowed to delete his account
            Queries.DeleteAccount(Program.connection, input_params.token);
            return new DataResponse(S_OK, DataFormat.Text, "Account Deleted");
        }
    }
}
