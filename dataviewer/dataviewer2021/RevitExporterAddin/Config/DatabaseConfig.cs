using System;

namespace RevitExporterAddin.Config
{
    /// <summary>
    /// Конфигурация подключения к базе данных CivilX
    /// </summary>
    public static class DatabaseConfig
    {
        /// <summary>
        /// Хост базы данных
        /// </summary>
        public static string Host => "localhost";

        /// <summary>
        /// Имя базы данных
        /// </summary>
        public static string DatabaseName => "u3279080_CivilX_users";

        /// <summary>
        /// Имя пользователя БД
        /// </summary>
        public static string Username => "u3279080_civilx_user";

        /// <summary>
        /// Пароль БД
        /// </summary>
        public static string Password => "!Grosheva78";

        /// <summary>
        /// Строка подключения к MySQL
        /// </summary>
        public static string ConnectionString => 
            $"Server={Host};Database={DatabaseName};Uid={Username};Pwd={Password};Charset=utf8mb4;";

        /// <summary>
        /// Строка подключения без указания базы данных (для создания БД)
        /// </summary>
        public static string ConnectionStringWithoutDatabase => 
            $"Server={Host};Uid={Username};Pwd={Password};Charset=utf8mb4;";
    }
}






