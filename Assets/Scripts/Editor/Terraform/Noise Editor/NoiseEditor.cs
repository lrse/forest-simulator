using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class NoiseEditor : EditorWindow {
    private Image _texture;
    private FloatField _scale;
    private SliderInt _octaves;
    private Slider _persistance;
    private FloatField _lacunarity;
    private IntegerField _seed;
    private Vector2Field _offset;
    private Button _load;
    private Button _save;
    private Button _save_texture;

    private NoiseData _noiseData;


    [MenuItem("Tools/Noise Editor")]
    public static void ShowEditor() {
        NoiseEditor wnd = GetWindow<NoiseEditor>();
        wnd.titleContent = new GUIContent("Noise Editor");
    }

    public void CreateGUI() {
        // Document root
        VisualElement root = rootVisualElement;

        // Import UXML
        VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/Editor/Terraform/Noise Editor/NoiseEditor.uxml");
        VisualElement labelFromUXML = visualTree.Instantiate();
        root.Add(labelFromUXML);

        // Apply style sheet
        StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/Editor/Terraform/Noise Editor/NoiseEditor.uss");
        root.styleSheets.Add(styleSheet);

        RegisterUIElements();
        RegisterCallbacks();

        DrawTexture();
    }

    private void RegisterUIElements() {
        _texture = rootVisualElement.Query<Image>("texture");
        _scale = rootVisualElement.Query<FloatField>("scale");
        _octaves = rootVisualElement.Query<SliderInt>("octaves");
        _persistance = rootVisualElement.Query<Slider>("persistance");
        _lacunarity = rootVisualElement.Query<FloatField>("lacunarity");
        _seed = rootVisualElement.Query<IntegerField>("seed");
        _offset = rootVisualElement.Query<Vector2Field>("offset");
        _load = rootVisualElement.Query<Button>("load");
        _save = rootVisualElement.Query<Button>("save");
        _save_texture = rootVisualElement.Query<Button>("save-texture");
    }

    private void RegisterCallbacks() {
        ObjectField noiseDataInput = rootVisualElement.Query<ObjectField>("noiseData");
        noiseDataInput.objectType = typeof(NoiseData);
        noiseDataInput.RegisterValueChangedCallback(evt => _noiseData = evt.newValue as NoiseData);
        _scale.RegisterCallback<ChangeEvent<float>>(OnValueChange);
        _octaves.RegisterCallback<ChangeEvent<int>>(OnValueChange);
        _persistance.RegisterCallback<ChangeEvent<float>>(OnValueChange);
        _lacunarity.RegisterCallback<ChangeEvent<float>>(OnValueChange);
        _seed.RegisterCallback<ChangeEvent<int>>(OnValueChange);
        _offset.RegisterCallback<ChangeEvent<Vector2>>(OnValueChange);
        _load.clicked += LoadNoiseData;
        _save.clicked += SaveNoiseData;
        _save_texture.clicked += SaveTexture;
    }

    private void DrawTexture() {
        float[,] data = Noise.GenerateNoiseMap(256, _seed.value, _scale.value, _octaves.value, _persistance.value, _lacunarity.value, _offset.value);
        Texture2D tex = TextureGenerator.TextureFromHeightMap(data);
        tex.filterMode = FilterMode.Bilinear;
        _texture.image = tex;
        _texture.scaleMode = ScaleMode.StretchToFill;
    }

    private void OnValueChange<T>(ChangeEvent<T> evt) {
        DrawTexture();
    }

    private void LoadNoiseData() {
        if (_noiseData == null) return;

        _scale.SetValueWithoutNotify(_noiseData.scale);
        _octaves.SetValueWithoutNotify(_noiseData.octaves);
        _persistance.SetValueWithoutNotify(_noiseData.persistance);
        _lacunarity.SetValueWithoutNotify(_noiseData.lacunarity);
        _seed.SetValueWithoutNotify(_noiseData.seed);
        _offset.SetValueWithoutNotify(_noiseData.offset);

        DrawTexture();
    }

    private void SaveNoiseData() {
        if (_noiseData == null) {
            Debug.Log("Nothing");
            // TODO: Create data file
        } else {
            _noiseData.seed = _seed.value;
            _noiseData.scale = _scale.value;
            _noiseData.octaves = _octaves.value;
            _noiseData.persistance = _persistance.value;
            _noiseData.lacunarity = _lacunarity.value;
            _noiseData.offset = _offset.value;
            EditorUtility.SetDirty(_noiseData);
        }
    }

    private void SaveTexture() {
        if (_noiseData != null) {
            float[,] data = Noise.GenerateNoiseMap(256, _seed.value, _scale.value, _octaves.value, _persistance.value, _lacunarity.value, _offset.value);
            Texture2D tex = TextureGenerator.TextureFromHeightMap(data);
            TextureGenerator.SaveTextureAsPNG(tex);
        }
    }
}