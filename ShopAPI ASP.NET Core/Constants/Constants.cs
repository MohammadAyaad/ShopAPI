using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopAPI.Constants
{
    public static class DebugConstants
    {

    }
    public static class LogConstants
    {
        /// <summary>
        /// MOST IMPORTANT IMMEDIATE ATTENTION REQUIRED
        /// </summary>
        public const string Log_Keyword_ImportanceLVL5 = "L5"; //
        /// <summary>
        /// VERY IMPORTANT ATTENTION REQUIRED
        /// </summary>
        public const string Log_Keyword_ImportanceLVL4 = "L4"; //
        /// <summary>
        /// IMPORTANT ATTENTION SOON REQUIRED
        /// </summary>
        public const string Log_Keyword_ImportanceLVL3 = "L3"; //
        /// <summary>
        /// NOT THAT IMPORTANT (INTERESTING)
        /// </summary>
        public const string Log_Keyword_ImportanceLVL2 = "L2"; //
        /// <summary>
        /// NOT IMPORTANT (UNCOMMON)
        /// </summary>
        public const string Log_Keyword_ImportanceLVL1 = "L1"; //
        /// <summary>
        /// COMMON
        /// </summary>
        public const string Log_Keyword_ImportanceLVL0 = "L0"; //
    }
    public static class ShopAPIDatabaseConstants
    {
        public const string TN_PRODUCTS = "products";
        public const string TN_PRODUCTS_CN_ID = "id";
        public const string TN_PRODUCTS_CN_NAME = "name";
        public const string TN_PRODUCTS_CN_DESCRIPTION = "description";
        public const string TN_PRODUCTS_CN_IMAGE = "img";
        public const string TN_PRODUCTS_CN_RATING = "rating";
        public const string TN_PRODUCTS_CN_TIMES_RATED = "times_rated";

        public const string TN_PRODUCT_VARIANTS = "product_variants";
        public const string TN_PRODUCT_VARIANTS_CN_ID = "id";
        public const string TN_PRODUCT_VARIANTS_CN_PRODUCT_ID = "product_id";
        public const string TN_PRODUCT_VARIANTS_CN_PROPERTIES = "variant_properties";
        public const string TN_PRODUCT_VARIANTS_CN_PRICE = "price";
        public const string TN_PRODUCT_VARIANTS_CN_QUANTITY = "quantity";

        public const string TN_PACKAGES = "packages";
        public const string TN_PACKAGES_CN_ID = "id";
        public const string TN_PACKAGES_CN_NAME = "name";
        public const string TN_PACKAGES_CN_DESCRIPTION = "description";
        public const string TN_PACKAGES_CN_IMAGE = "img";
        public const string TN_PACKAGES_CN_RATING = "rating";
        public const string TN_PACKAGES_CN_TIMES_RATED = "times_rated";

        public const string TN_PACKAGE_CONTENT = "package_content";
        public const string TN_PACKAGE_CONTENT_CN_PACKAGE_ID = "package_id";
        public const string TN_PACKAGE_CONTENT_CN_PRODUCT_ID = "product_id";
        public const string TN_PACKAGE_CONTENT_CN_ID = "id";

        public const string TN_USERS = "users";
        public const string TN_USERS_CL_EMAIL = "email";
        public const string TN_USERS_CL_PASSHASH = "password";
        public const string TN_USERS_CL_USERNAME = "username";
        public const string TN_USERS_CL_LASTNAME = "lastname";
        public const string TN_USERS_CL_ROLE = "role";
        public const string TN_USERS_CL_DATE_OF_BIRTH = "date_of_birth";

        public const string TN_LOGIN_USERS_JWT = "login_secret_jwt";
        public const string TN_LOGIN_USERS_JWT_CL_TOKEN_ID = "token_uuid";
        public const string TN_LOGIN_USERS_JWT_CL_TOKEN_EMAIL = "email";
        public const string TN_LOGIN_USERS_JWT_CL_TOKEN_SECRET_KEY = "secret_key";
        public const string TN_LOGIN_USERS_JWT_CL_TOKEN_ENCRYPTION_KEY = "encryption_key";

        public const string TN_PRODUCT_RATINGS = "product_ratings";
        public const string TN_PRODUCT_RATINGS_CL_ID = "rate_id";
        public const string TN_PRODUCT_RATINGS_CL_EMAIL = "rater_email";
        public const string TN_PRODUCT_RATINGS_CL_PRODUCT_ID = "product_id";
        public const string TN_PRODUCT_RATINGS_CL_VARIANT_ID = "variant_id";
        public const string TN_PRODUCT_RATINGS_CL_RATED_AT = "rated_at";
        public const string TN_PRODUCT_RATINGS_CL_COMMENT = "comment";
        public const string TN_PRODUCT_RATINGS_CL_RATING = "rating";
        public const string TN_PRODUCT_RATINGS_CL_UPVOTES = "upvotes";
        public const string TN_PRODUCT_RATINGS_CL_DOWNVOTES = "downvotes";
        public const string TN_PRODUCT_RATINGS_CL_SCORE = "score";

        public const string TN_PACKAGE_RATINGS = "package_ratings";
        public const string TN_PACKAGE_RATINGS_CL_ID = "rate_id";
        public const string TN_PACKAGE_RATINGS_CL_EMAIL = "rater_email";
        public const string TN_PACKAGE_RATINGS_CL_PACKAGE_ID = "package_id";
        public const string TN_PACKAGE_RATINGS_CL_RATED_AT = "rated_at";
        public const string TN_PACKAGE_RATINGS_CL_COMMENT = "comment";
        public const string TN_PACKAGE_RATINGS_CL_RATING = "rating";
        public const string TN_PACKAGE_RATINGS_CL_UPVOTES = "upvotes";
        public const string TN_PACKAGE_RATINGS_CL_DOWNVOTES = "downvotes";
        public const string TN_PACKAGE_RATINGS_CL_SCORE = "score";

        public const string TN_API_EXCEPTIONS_LOG = "api_exceptions_log";
        public const string TN_API_EXCEPTIONS_LOG_CL_ID = "id";
        public const string TN_API_EXCEPTIONS_LOG_CL_MESSAGE = "message";
        public const string TN_API_EXCEPTIONS_LOG_CL_STACKTRACE = "stacktrace";
        public const string TN_API_EXCEPTIONS_LOG_CL_SOURCE = "source";
        public const string TN_API_EXCEPTIONS_LOG_CL_THROWED_AT = "throwed_at";
        public const string TN_API_EXCEPTIONS_LOG_CL_INNER_EXCEPTION_ID = "inner_id";
        public const string TN_API_EXCEPTIONS_LOG_CL_IMPORTANCE = "importance";

        public const string TN_API_REQUESTS_LOG = "api_requests_log";
        public const string TN_API_REQUESTS_LOG_CL_ID = "uuid";
        public const string TN_API_REQUESTS_LOG_CL_METHOD = "method";
        public const string TN_API_REQUESTS_LOG_CL_PATH = "path";
        public const string TN_API_REQUESTS_LOG_CL_HANDLER = "handler";
        public const string TN_API_REQUESTS_LOG_CL_BODY = "body";
        public const string TN_API_REQUESTS_LOG_CL_RESPONSE_STATUS_CODE = "res_status_code";
        public const string TN_API_REQUESTS_LOG_CL_RESPONSE_STATUS = "res_status";
        public const string TN_API_REQUESTS_LOG_CL_RESPONSE_BODY = "res_body";
        public const string TN_API_REQUESTS_LOG_CL_REQUESTED_AT = "requested_at";
        public const string TN_API_REQUESTS_LOG_CL_PROCESSING_TIME = "processing_time";


    }

    public static class QueryParametersConstants
    {
        public const string QRY_SEED = "seed";
        public const string QRY_NEXT = "n";
        public const string QRY_PREVIOUS = "p";
        public const string QRY_INDEX = "i";
        public const string QRY_LIMIT = "l";
        public const string QRY_SEARCH = "s";
        public const string QRY_BEGINING = "b";
        public const string QRY_PRICE_TOP_FILTER = "fpt";
        public const string QRY_PRICE_BOTTOM_FILTER = "fpb";
        public const string QRY_INSTOCK_FILTER = "fs";
        public const string QRY_ORDERBY = "fo";
        public const string QRY_CONTAINS_PRODUCTS_FILTER = "fcp";
        public const string QRY_PACKAGE_LIST = "pl";
        public const string QRY_PRODUCT_LIST = "rl";
        public const string QRY_EMAIL = "em";
        public const string QRY_PASSWORD = "pwd";
        public const string QRY_USERNAME = "un";
        public const string QRY_LASTNAME = "ln";
        public const string QRY_BIRTH_DATE = "bd";
    }

    public static class KeywordsConstants
    {
        public const string KW_EMAIL = "email";
        public const string KW_PASSWORD = "password";
        public const string KW_USERNAME = "username";
        public const string KW_LASTNAME = "lastname";
        public const string KW_BIRTH_DATE = "birthday";
        public const string KW_PERMISSIONS = "permissions";
        public const string KW_TOKEN = "token";
        public const string KW_INFO = "info";
    }
}
