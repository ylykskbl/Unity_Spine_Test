// 测试用：初始化时加载第一个皮肤，按数字键 1～7 替换当前皮肤（不新增角色）
using Spine;
using Spine.Unity;
using UnityEngine;

public class SpineTestSpawner : MonoBehaviour
{
    public CharacterFactory characterFactory;
    [Tooltip("唯一角色的显示位置（世界坐标）")]
    public Vector3 spawnPosition = new Vector3(0, -3f, 0);

    private GameObject currentCharacter;
    private SpineAnimationManager currentAnimManager;

    void Start()
    {
        if (characterFactory == null) return;

        // 从骨骼数据中打印所有皮肤名称，便于确认按键应使用的名称
        if (characterFactory.commonSkeletonData != null)
        {
            SkeletonData data = characterFactory.commonSkeletonData.GetSkeletonData(true);
            if (data != null && data.Skins != null && data.Skins.Count > 0)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                for (int i = 0; i < data.Skins.Count; i++)
                    sb.Append(i > 0 ? ", " : "").Append('"').Append(data.Skins.Items[i].Name).Append('"');
                Debug.Log("[SpineTestSpawner] 骨骼中的皮肤名称（按此名称切换）: " + sb.ToString());
            }
        }

        // 默认加载第一个皮肤
        Vector3 pos = spawnPosition;
        pos.y = -3f;
        currentCharacter = characterFactory.CreateCharacter("1", pos);
        if (currentCharacter != null)
            currentAnimManager = currentCharacter.GetComponent<SpineAnimationManager>();
    }

    void Update()
    {
        if (characterFactory == null) return;

        int keyPressed = -1;
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) keyPressed = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) keyPressed = 2;
        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) keyPressed = 3;
        else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) keyPressed = 4;
        else if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5)) keyPressed = 5;
        else if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6)) keyPressed = 6;
        else if (Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Keypad7)) keyPressed = 7;

        if (keyPressed < 1) return;

        Vector3 pos = spawnPosition;
        pos.y = -3f;

        // 若尚未有角色，先创建；否则销毁当前角色并用新皮肤重新创建（与初始化同路径，保证能切换）
        if (currentCharacter != null)
        {
            Destroy(currentCharacter);
            currentCharacter = null;
            currentAnimManager = null;
        }

        currentCharacter = characterFactory.CreateCharacter(keyPressed.ToString(), pos);
        if (currentCharacter != null)
            currentAnimManager = currentCharacter.GetComponent<SpineAnimationManager>();
        Debug.Log($"[SpineTestSpawner] 切换皮肤: " + keyPressed);
    }
}
