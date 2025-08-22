using UnityEngine;
using UnityEngine.Events;
public class FirstPersonCamera : MonoBehaviour
{
    [Header("Rotation")]
    [Range(-90, 90f)] [SerializeField] private float _yMinRotation = -60f;
    [Range(-90f, 90f)] [SerializeField] private float _yMaxRotation = 75f;

    

    [SerializeField] Transform _playerHead;
    private float _mouseY;
    [SerializeField]FirstPersonPlayer _pj;
    IFPPlayer _Ipj;
    public UnityEvent<LayerMask> OnLayerMaskRequied;

    public void Awake()
    {
        _Ipj = _pj.GetComponent<IFPPlayer>();
        _Ipj.OnAxisMouseRequied += Rotate;
    }



    private void LateUpdate()
    {
        Movement();
    }

    private void Movement()
    {
        transform.position = _playerHead.position;
    }

    public void Rotate(float xAxis, float yAxis)
    {
        _mouseY += yAxis;
        _mouseY = Mathf.Clamp(_mouseY, _yMinRotation, _yMaxRotation);
        transform.rotation = Quaternion.Euler(-_mouseY, xAxis, 0f);
        
    }

    
}
