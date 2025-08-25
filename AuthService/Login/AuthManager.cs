using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


public class AuthManager : MonoBehaviour
{
    [SerializeField] private InputField emailInput;
    [SerializeField] private InputField passwordInput;

    // 注册按钮点击事件
    public void OnRegisterClick()
    {
        StartCoroutine(RegisterRequest(emailInput.text, passwordInput.text));
    }

    // 登录按钮点击事件
    public void OnLoginClick()
    {
        StartCoroutine(LoginRequest(emailInput.text, passwordInput.text));
    }

    // 发送注册请求
    private IEnumerator RegisterRequest(string email, string password)
    {
        WWWForm form = new WWWForm();
        form.AddField("email", email);
        form.AddField("password", password);

        using (UnityWebRequest request = UnityWebRequest.Post("http://localhost:8080/auth/register", form))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("注册成功：" + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("注册失败：" + request.error);
            }
        }
    }

    // 发送登录请求
    private IEnumerator LoginRequest(string email, string password)
    {
        WWWForm form = new WWWForm();
        form.AddField("email", email);
        form.AddField("password", password);

        using (UnityWebRequest request = UnityWebRequest.Post("http://localhost:8080/auth/login", form))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("登录成功：" + request.downloadHandler.text);
                // 跳转至游戏场景
            }
            else
            {
                Debug.LogError("登录失败：" + request.error);
            }
        }
    }
}