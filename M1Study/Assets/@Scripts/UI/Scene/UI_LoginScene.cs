using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using WebPacket;

public class UI_LoginScene : UI_Scene
{
    enum Buttons
    {
        FacebookButton,
        GuestButton,
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindButtons(typeof(Buttons));

        GetButton((int)Buttons.FacebookButton).gameObject.BindEvent(OnClickFacebookButton);
        return true;
    }

    public void OnClickFacebookButton(PointerEventData evt)
    {
        Managers.Auth.TryFacebookLogin((result) => OnLoginSucess(result, Define.EProviderType.Facebook));
    }

    public void OnLoginSucess(AuthResult authResult, Define.EProviderType providerType)
    {
        LoginAccountPacketReq req = new LoginAccountPacketReq()
        {
            userId = authResult.uniqueId,
            token = authResult.token
        };

        string url = "";

        switch (providerType)
        {
            case Define.EProviderType.Guest:
                url = "guest";
                break;
            case Define.EProviderType.Facebook:
                url = "facebook";
                break;
            case Define.EProviderType.Google:
                url = "google";
                break;
            default:
                return;
        }

        Managers.Web.SendPostRequest<LoginAccountPacketRes>($"api/account/login/{url}", req, (res) =>
        {
            if (res.success)
            {
                Debug.Log("Login Success");
                Debug.Log($"AccountDbId: {res.accountDbId}");
                Debug.Log($"JWT: {res.jwt}");

                // TODO
                Debug.Log("Try to Connect to GameServer...");
            }
            else
            {
                Debug.Log("Login Failed");
            }
        });
    }
}
