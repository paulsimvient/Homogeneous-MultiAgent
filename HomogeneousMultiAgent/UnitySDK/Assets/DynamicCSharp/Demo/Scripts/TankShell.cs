using UnityEngine;

namespace DynamicCSharp.Demo
{
    /// <summary>
    /// Represents a single shell that can be fired by the <see cref="TankController"/> 
    /// </summary>
    public class TankShell : MonoBehaviour
    {
        // Private
        private Vector2 startPosition = Vector2.zero;
        private Vector2 heading = Vector2.zero;
        private bool hit = false;

        // Public
        /// <summary>
        /// How fast the shell moves.
        /// </summary>
        public float speed = 2;

        // Methods
        /// <summary>
        /// Called by the tank controller to move the shell along its trajectory.
        /// </summary>
        /// <returns>True when the shell collides with an object</returns>
        public bool Step()
        {
            // Check for hit
            if (hit == true)
                return true;

            // Failsafe - if the bullet goes too far then mark it as dead to avoid an infinite wait
            if (Vector2.Distance(startPosition, transform.position) > 20)
                hit = true;

            // Move the bullet
            transform.Translate(heading * (speed * Time.deltaTime));

            // Bullet has not died
            return false;
        }

        /// <summary>
        /// Kill the shell object.
        /// </summary>
        public void Destroy()
        {
            // Destroy the object
            Destroy(gameObject);
        }

        /// <summary>
        /// Called by Unity.
        /// </summary>
        /// <param name="collision">The collider that was hit</param>
        public void OnCollisionEnter2D(Collision2D collision)
        {
            Collider2D collider = collision.collider;

            if (collider.name == "DamagedWall")
            {
                // Destroy the hit wall
                Destroy(collider.gameObject);

                // The bullet should be destroyed
                hit = true;
            }
            else if (collider.name == "Wall")
            {
                // The bullet should be destroyed
                hit = true;
            }
        }

        /// <summary>
        /// Create a tank shell using the specified values.
        /// </summary>
        /// <param name="prefab">The shell prefab to instantiate</param>
        /// <param name="startPosition">The start position of the shell</param>
        /// <param name="heading">The direction vector that the shell is heading in</param>
        /// <returns>An instance of <see cref="TankShell"/></returns>
        public static TankShell Shoot(GameObject prefab, Vector2 startPosition, Vector2 heading)
        {
            // Create a shell
            GameObject shell = Instantiate(prefab, startPosition, Quaternion.identity) as GameObject;

            // Get the script
            TankShell script = shell.GetComponent<TankShell>();

            // Check for error
            if (script == null)
            {
                Destroy(shell);
                return null;
            }

            // Store the heading
            script.startPosition = startPosition;
            script.heading = heading;

            return script;
        }
    }
}