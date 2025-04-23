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
    
    private Model runtimeModel;
    private IWorker worker;

    public float time;
    public float botMoney;
    public float botPlanted;
    public float botReady;
    public int botGrowthTime;
    public float opponentPlanted;
    public float opponentReady;
    public int opponentGrowthTime;

    private readonly string[] actions = { "Mưa", "Sấm sét", "Bảo vệ", "Chuột", "Sóng thần", "Không mua" };
    private readonly float[] actionCosts = { 30f, 15f, 30f, 30f, 40f, 0f }; // Chi phí của các hành động

    private const float MAX_PLOTS = 12 * 12;
    private const float MAX_TIME = 180f;
    private const float MAX_GROWTH_TIME = 60f;

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
            runtimeModel = ModelLoader.Load(modelAsset);
            if (runtimeModel == null)
            {
                Debug.LogError("Failed to load ONNX model.");
                return;
            }

            worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, runtimeModel);
            if (worker == null)
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
        if (worker == null)
        {
            Debug.LogWarning("Worker not initialized. Cannot make decision.");
            return "Không mua";
        }

        // Chuẩn hóa input theo đúng Python
        float[] normFactors = new float[]
        {
            MAX_TIME,
            100f, // lưu ý: chỉ chia 100 chứ không clamp botMoney
            MAX_PLOTS,
            MAX_PLOTS,
            MAX_GROWTH_TIME,
            MAX_PLOTS,
            MAX_PLOTS,
            MAX_GROWTH_TIME
        };

        float[] rawInput = new float[]
        {
            time,
            botMoney,
            botPlanted,
            botReady,
            botGrowthTime,
            opponentPlanted,
            opponentReady,
            opponentGrowthTime
        };

        float[] inputData = new float[8];
        for (int i = 0; i < 8; i++)
        {
            inputData[i] = rawInput[i] / normFactors[i];
        }

        using var inputTensor = new Tensor(new TensorShape(1, 8), inputData);

        try
        {
            worker.Execute(inputTensor);
            Tensor outputTensor = worker.PeekOutput();
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

            string predictedAction = actions[maxIndex];

            // Kiểm tra lại điều kiện tiền
            float recoveredBotMoney = inputData[1] * 100f; // khôi phục giá trị botMoney thực tế

            if (recoveredBotMoney < actionCosts[maxIndex])
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
        time = (int)Time.time;
        botPlanted = 0f;
        botReady = 0f;
        opponentPlanted = 0f;
        opponentReady = 0f;

        botMoney = BotController.Instance.score;
        int botTimeGrow = 0, playerTimeGrow = 0;

        foreach (Transform child in map2)
        {
            if (child.childCount > 0)
            {
                var plant = child.GetComponentInChildren<Plant>();
                botPlanted++;
                botTimeGrow += plant.growTimer;
                if (plant.isReadyToHarvest)
                    botReady++;
            }
        }

        foreach (Transform child in map1)
        {
            if (child.childCount > 0)
            {
                var plant = child.GetComponentInChildren<Plant>();
                opponentPlanted++;
                playerTimeGrow += plant.growTimer;
                if (plant.isReadyToHarvest)
                    opponentReady++;
            }
        }

        botGrowthTime = botPlanted > 0 ? Mathf.RoundToInt((botTimeGrow / botPlanted) * 5) : 0;
        opponentGrowthTime = opponentPlanted > 0 ? Mathf.RoundToInt((playerTimeGrow / opponentPlanted) * 5) : 0;

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
        worker?.Dispose();
        Debug.Log("BotDecisionMaker destroyed.");
    }
}