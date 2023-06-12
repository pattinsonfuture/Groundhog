using Groundhog.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Groundhog.Services
{
    public class MongoService
    {
        private IMongoDatabase _database;
        private IMongoCollection<BsonDocument> _collection;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _changeStreamTask;

        public async Task ConnectAsync(string connectionString, string databaseName, string collectionName, LoggingService _logger)
        {
            try
            {
                // 连接到 MongoDB
                var client = new MongoClient(connectionString);
                _database = client.GetDatabase(databaseName);
                _collection = _database.GetCollection<BsonDocument>(collectionName);

                // 记录连接成功的日志，傳入Discord.LogMessage
                await _logger.LogInfoAsync("MongoService", "成功連結到 MongoDB");
            }
            catch (Exception ex)
            {
                // 记录连接过程中的错误日志
                await _logger.LogErrorAsync("MongoService", ex.Message);
                throw; // 重新抛出异常以指示连接失败
            }
        }


        public void StartChangeStream()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            // 启动 Change Stream 任务
            _changeStreamTask = Task.Run(async () => await MonitorChangeStream(cancellationToken), cancellationToken);
        }

        public void StopChangeStream()
        {
            // 停止 Change Stream 任务
            _cancellationTokenSource.Cancel();
            _changeStreamTask.Wait();
        }

        private async Task MonitorChangeStream(CancellationToken cancellationToken)
        {
            // 设置 Change Stream 的选项
            var options = new ChangeStreamOptions
            {
                FullDocument = ChangeStreamFullDocumentOption.UpdateLookup
            };

            // 创建 Change Stream
            var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<BsonDocument>>().Match(x => x.OperationType == ChangeStreamOperationType.Update);
            using (var changeStreamCursor = await _collection.WatchAsync(pipeline, options, cancellationToken))
            {
                while (!cancellationToken.IsCancellationRequested && await changeStreamCursor.MoveNextAsync(cancellationToken))
                {
                    var batch = changeStreamCursor.Current;
                    foreach (var change in batch)
                    {
                        // 处理变更事件
                        var document = change.FullDocument;
                        Console.WriteLine($"Received update: {document.ToJson()}");

                        // 解析变更事件并执行相应的操作
                        var pluginName = document.GetValue("name").AsString;
                        var isEnabled = document.GetValue("isEnabled").AsBoolean;

                        // 重新注册插件
                        //var pluginService = services.GetRequiredService<PluginService>();
                        //await pluginService.InstallPluginsAsync();
                    }
                }
            }
        }
        /// <summary>
        ///  搜尋過濾參數、取得插件列表
        /// </summary>
        /// <param filter=></param>
        /// <returns></returns>
        public async Task<List<IPlugin>> GetPluginsAsync(FilterDefinition<BsonDocument> filter)
        {
            var documents = await _collection.Find(filter).ToListAsync();
            var plugins = new List<IPlugin>();
            foreach (var document in documents)
            {
                var plugin = BsonSerializer.Deserialize<IPlugin>(document);
                plugins.Add(plugin);
            }
            return plugins;
        }


    }
}
