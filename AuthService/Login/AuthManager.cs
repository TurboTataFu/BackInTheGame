using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


public class AuthManager : MonoBehaviour
{
    [SerializeField] private InputField emailInput;
    [SerializeField] private InputField passwordInput;

    // ע�ᰴť����¼�
    public void OnRegisterClick()
    {
        StartCoroutine(RegisterRequest(emailInput.text, passwordInput.text));
    }

    // ��¼��ť����¼�
    public void OnLoginClick()
    {
        StartCoroutine(LoginRequest(emailInput.text, passwordInput.text));
    }

    // ����ע������
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
                Debug.Log("ע��ɹ���" + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("ע��ʧ�ܣ�" + request.error);
            }
        }
    }

    // ���͵�¼����
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
                Debug.Log("��¼�ɹ���" + request.downloadHandler.text);
                // ��ת����Ϸ����
            }
            else
            {
                Debug.LogError("��¼ʧ�ܣ�" + request.error);
            }
        }
    }
}