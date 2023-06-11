using Groundhog.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Groundhog.Services
{
    public class DatabaseService
    {
        private IMongoDatabase _database;
        private IMongoCollection<BsonDocument> _collection;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _changeStreamTask;

        public DatabaseService(string connectionString, string databaseName, string collectionName)
        {
            // 连接到 MongoDB
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
            _collection = _database.GetCollection<BsonDocument>(collectionName);
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
                        // 执行更新逻辑...
                    }
                }
            }
        }

        public async Task<List<IPlugin>> GetEnabledPluginsAsync()
        {
            var plugins = new List<IPlugin>();

            // 查询 enabledPlugins 数据
            var filter = Builders<BsonDocument>.Filter.Empty;
            var documents = await _collection.Find(filter).ToListAsync();

            // 将查询结果转换为 Plugin 对象
            foreach (var document in documents)
            {
                var plugin = new Plugin
                {
                    Name = document.GetValue("name").AsString,
                    // 其他字段...
                };

                plugins.Add(plugin);
            }

            return plugins;
        }

        // ...
    }
}
