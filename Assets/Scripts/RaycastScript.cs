using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class RaycastScript:MonoBehaviour
{
    [Range(1, 100)] public int distanceMultipler;
    public Sprite onItemCursor;
    public Image image;
    
    private Sprite _sprite;
    
    private Color _color;
    
    private RaycastHit _hit;

    private Vector2 _size;
    
    private Vector3 _startScale;
    private Vector3 _supposedScale;
    
    private GameObject _pickedCopy;
    private GameObject _pickedObject;
    
    private Rigidbody _pickedBody;
    
    private bool _isPicked;
    private bool _isTargeted;
    
    private void Awake()
    {
        // Writing initial cursor parameters
        _color = image.color;
        _sprite = image.sprite;
        _size = image.GetComponent<RectTransform>().rect.size;
    }

    private void Start()
    {
        // Starting routines to find items and moving pocked object
        StartCoroutine(PushRaycastRoutine());
        StartCoroutine(MovePickedObjectRoutine());
    }

    private void Update()
    {
        // Pressing E picks item, pressing again will drop
        if (_isPicked && Input.GetKeyDown(KeyCode.E))
        {
            _isPicked = false;
        }
        
        if (_isTargeted && Input.GetKeyDown(KeyCode.E))
        {
            
            _pickedObject = _hit.collider.gameObject;
            _pickedObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            _pickedBody = _pickedObject.GetComponent<Rigidbody>();
            _isPicked = true;
            
        }
    }
    
    
    // Just visualisation of the function below
    
    
    
    private void OnDrawGizmos()
    {
        while (!_isPicked)
        {
            return;
        }

        if (!_isPicked || !Physics.Raycast(transform.position, transform.forward, out var pickHit)) return;
        
        Gizmos.DrawRay(pickHit.point, -transform.forward);
        float distance = Vector3.Distance(transform.position, pickHit.point);

        bool check = true;
            
        for (int i = 0; i < Mathf.RoundToInt(distance * distanceMultipler); i++)
        {
            Vector3 pos = Vector3.Lerp(transform.position, pickHit.point, i / (distance * distanceMultipler));
            float radius = _supposedScale.x / 2 * (i / (distance * distanceMultipler));

            Collider[] colliders = Physics.OverlapSphere(pos, radius);

            if (colliders.Any(col => col.CompareTag("Geometry")))
            {
                check = false;
            }

            if (!check)
            {
                break;
            }
                
            Gizmos.DrawWireSphere(pos, radius);
        }
    } 
    
    // Coroutine which business to move picked object
    private IEnumerator MovePickedObjectRoutine()
    {
        while (true)
        {
            yield return new WaitUntil(() => _isPicked);

            // Staff to keep picked object the same size it was before picking
            _startScale = _pickedObject.transform.localScale;
            Physics.Raycast(transform.position, transform.forward, out var pickHit);
            float distance = Vector3.Distance(transform.position, pickHit.point);
            float coef = distance/Vector3.Distance(transform.position, _pickedObject.transform.position);
            _pickedObject.transform.localScale = _startScale*coef;
            _pickedBody.MovePosition(pickHit.point);
            
            
            // Initial Variables and disabling physical properties
            _startScale = _pickedObject.transform.localScale;
            _supposedScale = _pickedObject.transform.localScale;
            _pickedBody.velocity = Vector3.zero;
            _pickedBody.useGravity = false;
            _pickedBody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
            Vector3 tempRay = Vector3.zero;
            Vector3 pos = _pickedObject.transform.position;
            
            
            while (_isPicked && (Physics.Raycast(transform.position, transform.forward, out pickHit)))
            {
                // If you keep cursor in same place nothing will change
                if (tempRay == pickHit.point)
                {
                    yield return null;
                    continue;
                }
                tempRay = pickHit.point;
                
                // Scale that picked object must have with no obstacles on its way. It is used to keep OverlapSphere radius same on the same distances casted
                _supposedScale = _startScale * Vector3.Distance(transform.position, pickHit.point) / distance;
                bool check = true;
                
                // Loop where we cast overlapping with idea to find possible obstacles and saving the last successful position
                for (int i = 0; i < Mathf.RoundToInt(distance * distanceMultipler); i++)
                {
                    pos = Vector3.Lerp(transform.position, pickHit.point, i / (distance * distanceMultipler));
                    float radius = _supposedScale.x / 2 * (i / (distance * distanceMultipler));

                    Collider[] colliders = Physics.OverlapSphere(pos, radius);

                    if (colliders.Any(col => col.CompareTag("Geometry")))
                    {
                        pos = Vector3.Lerp(transform.position, pickHit.point, (i-1) / (distance * distanceMultipler));
                        check = false;
                    }

                    if (!check)
                    {
                        break;
                    }
                }

                // Setting variables to move and grow/lower picked object scale
                _pickedBody.transform.position = pos;
                coef = Vector3.Distance(transform.position, pos)/ distance;
                _pickedObject.transform.localScale = _startScale*coef;
                _pickedBody.velocity = Vector3.zero;
                
                yield return null;
            }
            
            // Physical properties return
            _pickedBody.useGravity = true;
            _pickedBody.velocity = Vector3.zero;
            _pickedBody.constraints = RigidbodyConstraints.None;
            _pickedObject.layer = LayerMask.NameToLayer("Default");
        }
    }

    
    // Coroutine which launches rays all time in order to find pickable items
    private IEnumerator PushRaycastRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.1f);
        while (true)
        {
            yield return wait;
            
            // If raycast targets item cursor changes its icon
            if (Physics.Raycast(transform.position, transform.forward, out _hit) && _hit.collider.CompareTag("Item"))
            {
                _isTargeted = true;
                image.sprite = onItemCursor;
                image.color = Color.white;
                image.GetComponent<RectTransform>().sizeDelta = _size *4;
            }
            
            // Else, default icon sets
            else
            {
                _isTargeted = false;
                image.GetComponent<RectTransform>().sizeDelta = _size;
                image.color = _color;
                image.sprite = _sprite;
            }
        }
    }
}