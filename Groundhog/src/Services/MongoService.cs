using Groundhog.Interfaces;
using Microsoft.Extensions.Configuration;
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

        private readonly IConfiguration _configuration;
        private IMongoDatabase _database;
        private IMongoCollection<BsonDocument> _collection;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _changeStreamTask;
        private readonly LoggingService _logger;

        public MongoService(IConfiguration configuration, LoggingService logger)
        {
            _logger = logger;
            _configuration = configuration;

            try
            {
                // 連結到 MongoDB
                var client = new MongoClient(_configuration["MongoDBConnectionUri"]);
                _database = client.GetDatabase(_configuration["MongoDBDatabaseName"]);

                // 記錄到日誌
                _logger.LogInfoAsync("MongoService", "成功連結到 MongoDB").Wait();
            }
            catch (Exception ex)
            {
                // 紀錄連結 MongoDB 失敗的錯誤訊息
                _logger.LogErrorAsync("MongoService", ex.Message).Wait();
                throw; // 拋出異常
            }
        }

        public async Task<List<BsonDocument>> GetAllGuildsAsync()
        {
            // 連結到 Guilds Collection
            _collection = _database.GetCollection<BsonDocument>("guilds");
            // 取得所有 Guilds
            var guilds = await _collection.Find(new BsonDocument()).ToListAsync();

            return guilds;
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


    }
}
