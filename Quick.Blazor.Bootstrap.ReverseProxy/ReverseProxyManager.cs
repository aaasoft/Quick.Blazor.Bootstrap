using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Quick.Blazor.Bootstrap.ReverseProxy.Model;
using Quick.EntityFrameworkCore.Plus;
using Quick.Localize;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;

namespace Quick.Blazor.Bootstrap.ReverseProxy
{
    public class ReverseProxyManager : IProxyConfigProvider
    {
        public static ReverseProxyManager Instance { get; } = new ReverseProxyManager();
        private List<ReverseProxyRule> ruleList;
        public event EventHandler<string> RuleAdded;
        public event EventHandler<string> RuleRemoved;

        public bool IsEmpty()
        {
            lock (ruleList)
                return ruleList.Count == 0;
        }

        private class InMemoryConfig : IProxyConfig
        {
            private readonly CancellationTokenSource _cts = new CancellationTokenSource();

            public InMemoryConfig(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
            {
                Routes = routes;
                Clusters = clusters;
                ChangeToken = new CancellationChangeToken(_cts.Token);
            }

            public IReadOnlyList<RouteConfig> Routes { get; }

            public IReadOnlyList<ClusterConfig> Clusters { get; }

            public IChangeToken ChangeToken { get; }

            internal void SignalChange()
            {
                _cts.Cancel();
            }
        }

        private Dictionary<string, RouteConfig> routeDict = new Dictionary<string, RouteConfig>();
        private Dictionary<string, ClusterConfig> clusterDict = new Dictionary<string, ClusterConfig>();
        private volatile InMemoryConfig _config;
        public IProxyConfig GetConfig() => _config;
        public ReverseProxyRule[] GetRules(string keywords)
        {
            lock (ruleList)
                return ruleList.Where(t => string.IsNullOrEmpty(keywords) || t.Name.Contains(keywords)).ToArray();
        }

        public ReverseProxyManager()
        {
            _config = new InMemoryConfig(new RouteConfig[0], new ClusterConfig[0]);
        }

        private void Update()
        {
            var oldConfig = _config;
            _config = new InMemoryConfig(routeDict.Values.ToArray(), clusterDict.Values.ToArray());
            oldConfig.SignalChange();
        }

        public IReverseProxyBuilder Load(IReverseProxyBuilder builder)
        {
            ruleList = ConfigDbContext.CacheContext.Query<ReverseProxyRule>().ToList();
            foreach (var item in ruleList)
            {
                AddRule(item.Path, item.Url);
            }
            builder.Services.AddSingleton<IProxyConfigProvider>(this);
            return builder;
        }

        public IReverseProxyBuilder LoadWithoutData(IReverseProxyBuilder builder)
        {
            ruleList = new List<ReverseProxyRule>();
            builder.Services.AddSingleton<IProxyConfigProvider>(this);
            return builder;
        }

        public void AddRule(ReverseProxyRule rule)
        {
            lock (ruleList)
            {
                if (ruleList.Any(t => t.Path == rule.Path))
                    throw new ApplicationException(Locale.GetString("Rule with path[{0}] already exist.", rule.Path));
                ruleList.Add(rule);
            }
            AddRule(rule.Path, rule.Url);
        }

        public void RemoveRule(ReverseProxyRule rule)
        {
            lock (ruleList)
            {
                if (ruleList.Contains(rule))
                    ruleList.Remove(rule);
            }
            RemoveRule(rule.Path);
        }

        public bool Exists(string path)
        {
            lock (routeDict)
                return routeDict.ContainsKey(path);
        }

        public void AddRule(string path, string url)
        {
            string routeMatchPath = path;
            string transformPathRemovePrefix = path;
            if (path.EndsWith("/"))
            {
                routeMatchPath = path + "{**catch-all}";
                transformPathRemovePrefix = transformPathRemovePrefix.Substring(0, transformPathRemovePrefix.Length - 1);
            }

            lock (routeDict)
            {
                routeDict[path] = new RouteConfig()
                {
                    RouteId = "route" + path,
                    ClusterId = "cluster" + path,
                    Match = new RouteMatch { Path = routeMatchPath }
                }.WithTransformPathRemovePrefix(transformPathRemovePrefix);

                clusterDict[path] = new ClusterConfig()
                {
                    ClusterId = "cluster" + path,
                    Destinations = new Dictionary<string, DestinationConfig>(StringComparer.OrdinalIgnoreCase)
                        {
                            { "destination1", new DestinationConfig() { Address = url } }
                        }
                };
                Update();
            }
            RuleAdded?.Invoke(this, path);
        }

        public void RemoveRule(string path)
        {
            lock (routeDict)
            {
                if (routeDict.ContainsKey(path))
                    routeDict.Remove(path);
                if (clusterDict.ContainsKey(path))
                    clusterDict.Remove(path);
                Update();
            }
            RuleRemoved?.Invoke(this, path);
        }

        private string[] getRulePathArray()
        {
            lock (routeDict)
                return routeDict.Keys.ToArray();
        }

        public void RemoveRules(string prefix)
        {
            var pathList = getRulePathArray();
            foreach (var path in pathList)
            {
                if (path.StartsWith(prefix))
                    RemoveRule(path);
            }
        }

        public void RemoveAllRules()
        {
            foreach (var rule in GetRules(null))
            {
                RemoveRule(rule);
            }
        }
    }
}
