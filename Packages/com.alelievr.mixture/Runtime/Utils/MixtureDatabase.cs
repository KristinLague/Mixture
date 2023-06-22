using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEditor;

namespace Mixture
{
    public class MixtureDatabase : ScriptableObject
    {
        public static readonly string databaseResourcePath = "Mixture/Database";

        static MixtureDatabase instance;

        public List<MixtureGraph> realtimeGraphs = new List<MixtureGraph>();
        public List<MixtureGraph> staticGraphs = new List<MixtureGraph>();

        private static Dictionary<Texture, MixtureGraph> graphMap = new Dictionary<Texture, MixtureGraph>();

        void OnEnable()
        {
            GetGraphMap();  
        }

        private static void GetGraphMap()
        {
            // The mixture database is only available in build. In the editor we have directly load the graph with AssetDatabase (which is safer).
#if UNITY_EDITOR
            string[] guids = AssetDatabase.FindAssets("t:Texture2D");
            foreach (var id in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(id);
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                MixtureGraph graph = GetGraphFromTexture(tex);

                if (graph != null)
                {
                    graphMap.Add(tex,graph);
                }
            }
#else

            foreach (var graph in realtimeGraphs)
                AddGraph(graph);
            foreach (var graph in staticGraphs)
                AddGraph(graph);

            void AddGraph(MixtureGraph graph)
            {
                foreach (var outputTexture in graph.outputTextures)
                    graphMap[outputTexture] = graph;
            }
#endif
        }

        public static Dictionary<Texture, MixtureGraph> GetAllGraphs()
        {
            if (graphMap.Count == 0)
            {
                GetGraphMap();
            }

            return graphMap;
        }

        /// <summary>
        /// Get the graph from the mixture
        /// </summary>
        /// <param name="texture"></param>
        /// <returns></returns>
        public static MixtureGraph GetGraphFromTexture(Texture texture)
        {
            if (texture == null)
                return null;

// In the editor, we can directly use the AssetDatabase instead of relying on the resources.
#if UNITY_EDITOR
			string graphPath = UnityEditor.AssetDatabase.GetAssetPath(texture);

            if (!String.IsNullOrEmpty(graphPath))
                return UnityEditor.AssetDatabase.LoadAllAssetsAtPath(graphPath).OfType<MixtureGraph>().FirstOrDefault();
            else
                return null;

#else

            if (instance == null)
            {
                instance = Resources.Load<MixtureDatabase>(databaseResourcePath);
                if (instance == null)
                {
                    Debug.LogError("Mixture Database asset not found! Realtime Mixtures won't work as expected.");
                    return null;
                }
            }

            instance.graphMap.TryGetValue(texture, out var graph);
            return graph;
#endif
        }
    }
}