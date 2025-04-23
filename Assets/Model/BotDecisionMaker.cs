using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;
using Random = UnityEngine.Random;

public class BotDecisionMaker : MonoBehaviour
{
    [SerializeField] private NNModel modelAsset;
    [SerializeField] private Transform map1;
    [SerializeField] private Transform map2;
    
    private Model _runtimeModel;
    private IWorker _worker;

    private float _time;
    private float _botPlanted;
    private float _botReady;
    private int _botGrowthTime;
    private float _botMoney;
    private float _opponentPlanted;
    private float _opponentReady;
    private int _opponentGrowthTime;

    private readonly string[] _actions = { "Mưa", "Sấm sét", "Bảo vệ", "Chuột", "Sóng thần", "Không mua" };
    private readonly float[] _actionCosts = { 30f, 15f, 30f, 30f, 40f, 0f }; 

    private const float MaxPlots = 12 * 12;
    private const float MaxTime = 180f;
    private const float MaxGrowthTime = 60f;

    public static BotDecisionMaker Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        try
        {
            _runtimeModel = ModelLoader.Load(modelAsset);
            if (_runtimeModel == null)
            {
                Debug.LogError("Failed to load ONNX model.");
                return;
            }

            _worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, _runtimeModel);
            if (_worker == null)
            {
                Debug.LogError("Failed to create worker.");
                return;
            }

            Debug.Log("BotDecisionMaker initialized successfully.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error initializing BotDecisionMaker: {ex.Message}");
        }
    }

    public string MakeDecision()
    {
        if (_worker == null)
        {
            Debug.LogWarning("Worker not initialized. Cannot make decision.");
            return "Không mua";
        }

        float[] normFactors = 
        {
            MaxTime, 100f, MaxPlots, MaxPlots,
            MaxGrowthTime, MaxPlots, MaxPlots, MaxGrowthTime
        };

        float[] rawInput = 
        {
            _time, _botMoney, _botPlanted, _botReady, _botGrowthTime, 
            _opponentPlanted, _opponentReady, _opponentGrowthTime
        };

        float[] inputData = new float[8];
        for (int i = 0; i < 8; i++)
        {
            inputData[i] = rawInput[i] / normFactors[i];
        }

        using var inputTensor = new Tensor(new TensorShape(1, 8), inputData);

        try
        {
            _worker.Execute(inputTensor);
            Tensor outputTensor = _worker.PeekOutput();
            float[] outputData = outputTensor.ToReadOnlyArray();

            // Chọn hành động có giá trị logits lớn nhất
            int maxIndex = 0;
            float maxValue = outputData[0];

            for (int i = 1; i < outputData.Length; i++)
            {
                if (outputData[i] > maxValue)
                {
                    maxValue = outputData[i];
                    maxIndex = i;
                }
            }

            string predictedAction = _actions[maxIndex];

            // Kiểm tra lại điều kiện tiền
            float recoveredBotMoney = inputData[1] * 100f; // khôi phục giá trị botMoney thực tế

            if (recoveredBotMoney < _actionCosts[maxIndex])
            {
                predictedAction = "Không mua";
            }

            Debug.Log($"Input: [{string.Join(", ", inputData)}] -> Predicted Action: {predictedAction} (Logits: [{string.Join(", ", outputData)}])");
            return predictedAction;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error during inference: {ex.Message}");
            return "Không mua";
        }
    }
    
    public int UseItemWithModel()
    {
        _time = (int)Time.time;
        _botPlanted = 0f;
        _botReady = 0f;
        _opponentPlanted = 0f;
        _opponentReady = 0f;

        _botMoney = BotController.Instance.score;
        int botTimeGrow = 0, playerTimeGrow = 0;

        foreach (Transform child in map2)
        {
            if (child.childCount > 0)
            {
                var plant = child.GetComponentInChildren<Plant>();
                _botPlanted++;
                botTimeGrow += plant.growTimer;
                if (plant.isReadyToHarvest)
                    _botReady++;
            }
        }

        foreach (Transform child in map1)
        {
            if (child.childCount > 0)
            {
                var plant = child.GetComponentInChildren<Plant>();
                _opponentPlanted++;
                playerTimeGrow += plant.growTimer;
                if (plant.isReadyToHarvest)
                    _opponentReady++;
            }
        }

        _botGrowthTime = _botPlanted > 0 ? Mathf.RoundToInt((botTimeGrow / _botPlanted) * 5) : 0;
        _opponentGrowthTime = _opponentPlanted > 0 ? Mathf.RoundToInt((playerTimeGrow / _opponentPlanted) * 5) : 0;

        string action = MakeDecision();
        Debug.Log($"Bot decided: {action}");
        return GetIndexItem(action);
    }
    
    public int GetIndexItem(string action)
    {
        switch (action)
        {
            case "Mưa": return 1;
            case "Sấm sét": return 5;
            case "Bảo vệ": return 0;
            case "Chuột": return 4;
            case "Sóng thần": return 3;
            case "Không mua": return 10;
            default: return -1;
        }
    }

    void OnDestroy()
    {
        _worker?.Dispose();
        Debug.Log("BotDecisionMaker destroyed.");
    }
}