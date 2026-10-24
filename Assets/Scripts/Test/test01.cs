using System.Collections;
using Framework.Bootstrap;
using Framework.Services.SceneFlow;
using UnityEngine;

public class test01 : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(TestSceneFlowManager());
    }
    IEnumerator TestSceneFlowManager()
    {
        yield return new WaitForSeconds(2f);
        GameRoot.Instance.Services.Get<ISceneFlowService>().LoadScene("GameScene");
    }
}
