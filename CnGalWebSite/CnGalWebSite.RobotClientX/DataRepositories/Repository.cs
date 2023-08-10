
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace CnGalWebSite.RobotClientX.DataRepositories
{
  
    /// <summary>
    /// 此接口是所有仓储的约定，此接口仅作为约定，用于标识它们
    /// </summary>
    /// <typeparam name="TEntity">当前传入的仓储的实体类型</typeparam>
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        //内存仓储
        private List<TEntity> _repository = new();
        //索引
        private readonly string _index = typeof(TEntity).ToString().Split('.').Last().ToLower();
     
        //依赖注入服务
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public Repository(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;

            Load();
        }

        private void Load()
        {

            try
            {
                //尝试打开文件
                string path = Path.Combine(_webHostEnvironment.WebRootPath,"Data", $"{_index}.json");
               
                using StreamReader file = File.OpenText(path);
                JsonSerializer serializer = new();
                _repository.Clear();
                _repository.AddRange(serializer.Deserialize(file, typeof(List<TEntity>)) as List<TEntity> ?? new List<TEntity>());   
            }
            catch
            {
                Save();
            }

        }

        public void Delete(TEntity entity)
        {
            _ = _repository.Remove(entity);
            Save();
        }

        public List<TEntity> GetAll()
        {
            return _repository;
        }

        public TEntity Insert(TEntity entity)
        {
            _repository.Add(entity);
            Save();
            return entity;
        }

        public void Save()
        {
            string path = Path.Combine(_webHostEnvironment.WebRootPath, "Data", $"{_index}.json");

            using StreamWriter file = File.CreateText(path);
            JsonSerializer serializer = new();
            serializer.Serialize(file, _repository);
            file.Close();
            file.Dispose();
        }
    }
}
