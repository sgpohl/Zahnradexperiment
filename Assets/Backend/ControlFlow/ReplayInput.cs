using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.IO;

public class ReplayInput : BaseInput
{
    public override Vector2 mousePosition
    {
        get { return new Vector2(currentPoint.x, currentPoint.y); }
    }

    public override float GetAxisRaw(string axisName)
    {
        return base.GetAxisRaw(axisName);
    }

    public override bool GetMouseButton(int button)
    {
        if (button != 0)
            return false;
        return currentPoint.MouseDown;
    }

    public override bool GetMouseButtonDown(int button)
    {
        if (button != 0)
            return false;
        return currentPoint.MouseDown && currentPoint.MouseChange;
    }

    public override bool GetMouseButtonUp(int button)
    {
        if (button != 0)
            return false;
        return !currentPoint.MouseDown && currentPoint.MouseChange;
    }

    public override bool GetButtonDown(string buttonName)
    {
        return base.GetButtonDown(buttonName);
    }

    public override Touch GetTouch(int index)
    {
        /*var t = new Touch();
        t.position = new Vector2(currentPoint.x, currentPoint.y);
        t.pressure = 1.0f;
        t.radius = 1.0f;
        t.type = TouchType.Direct;*/
        return base.GetTouch(index);
    }

    private Data data = null;
    private Data.Point previousPoint;
    private Data.Point currentPoint;
    private Data.Point nextPoint;
    private System.Diagnostics.Stopwatch timer;
    public void Init(string replayFile)
    {
        data = new Data();
        timer = new System.Diagnostics.Stopwatch();
        data.Load(replayFile);

        previousPoint = data.Next();
        currentPoint = previousPoint;
        nextPoint = data.Next();

        timer.Start();
    }

    GameObject MousePointerSurrogate = null;
    private void Update()
    {
        if (data == null || !data.IsReady)
            return;
        if (data.HasReadToEnd)
        {
            Debug.Log("Replay finished");
            data = null;
            if(MousePointerSurrogate != null)
            {
                Destroy(MousePointerSurrogate);
                MousePointerSurrogate = null;
            }
            return;
        }
        if (timer.ElapsedMilliseconds > nextPoint.dt)
        {
            timer.Restart();
            previousPoint = nextPoint;
            currentPoint = previousPoint;
            nextPoint = data.Next();
        }
        else
            currentPoint.MouseChange = false;

        float posInterpolation = (float)timer.ElapsedMilliseconds / (float)nextPoint.dt;
        currentPoint.x = nextPoint.x * posInterpolation + previousPoint.x * (1.0f - posInterpolation);
        currentPoint.y = nextPoint.y * posInterpolation + previousPoint.y * (1.0f - posInterpolation);

        //Debug.Log(currentPoint.dt.ToString()+","+currentPoint.MouseDown.ToString()+",("+currentPoint.x.ToString("0.0")+","+currentPoint.y.ToString("0.0") +")");
        //Debug.Log(currentPoint.dt.ToString()+","+currentPoint.MouseDown.ToString()+",("+currentPoint.x.ToString()+","+currentPoint.y.ToString()+")");
        //Debug.Log(Input.mousePosition.ToString());

        if (MousePointerSurrogate == null)
        {
            MousePointerSurrogate = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            MousePointerSurrogate.transform.localScale = Vector3.one * 0.5f;
        }
        GameObject go = null;
        //if(currentPoint.MouseChange)    
        //    go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Vector2 tpos = Camera.main.ScreenToWorldPoint(new Vector3(Experiment.Input.mousePosition.x, Experiment.Input.mousePosition.y, Camera.main.nearClipPlane));
        Vector2 rpos = Camera.main.ScreenToWorldPoint(new Vector3(currentPoint.x, currentPoint.y, Camera.main.nearClipPlane));

        MousePointerSurrogate.transform.position = tpos;
        if (go != null)
        {
            go.transform.position = rpos;
            go.transform.localScale = Vector3.one * 0.4f;
        }
    }

    public class Data
    {
        private const byte MouseDragCode = byte.MaxValue; //magic number to please claude shannon
        private const byte IdleCode = byte.MinValue; //magic number to please claude shannon

        private const byte MouseDown = 1 << 0;      //Bitmask

        private const byte IdleLength = 1 << 6;

        private MemoryStream Stream = null;

        private const int BlockSize = sizeof(int) * 2 + 1;
        private byte[] block = new byte[BlockSize];

        private int xMin, xMax, yMin, yMax;

        public bool IsReady { get { return Stream != null; } }

        public struct Point
        {
            public float dt;
            public float x, y;
            public bool MouseDown, MouseChange;
        }
        public float EncodeFloat(uint b, uint range, int min, int max)
        {
            return ((float)b / range) * (max - min) + min;
        }
        /**
            * Encoding
            * Byte 0 == 0000 0000 -> Nothing (Fixed dt)
            * Byte 0 == 1111 1111 -> Drag (Fixed dt)
            * -> Byte 1&2 -> x,y
            * else
            * -> Byte 0 = dt
            * -> Byte 1-5 & 6,8 -> x,y
            * -> Byte 9 = MouseDown
            **/
        /*
    public Point Next()
    {
        //Debug.Log(xMin.ToString() + "," + xMax.ToString() + ",(" + yMin.ToString() + "," + yMax.ToString() + ")");
        //Debug.Log(EncodeFloat(1234, 1 << (8 * sizeof(int)), xMin, xMax).ToString() + "," + EncodeFloat(220, 254, xMin, xMax).ToString());
        var point = new Point { MouseDown = false, MouseChange = false };

        Stream.Read(block, 0, 1);
        point.dt = block[0];
        if (point.dt == IdleCode)
        {
            //Debug.Log(Stream.Position.ToString() + " Idle");
            point.dt = IdleLength;
            return point;
        }
        if (point.dt != MouseDragCode)
        {
            Stream.Read(block, 1, BlockSize - 1);

            int idx = 1;
            uint x = System.BitConverter.ToUInt32(block, idx); idx += sizeof(uint);
            uint y = System.BitConverter.ToUInt32(block, idx); idx += sizeof(uint);

            point.MouseDown = (y & MouseDown) > 0;
            y = y >> 1;
            point.MouseChange = true;

            //Debug.Log(Stream.Position.ToString() + " raw big in: " + x.ToString() + "," + y.ToString());
            point.x = EncodeFloat(x, uint.MaxValue, xMin, xMax);
            point.y = EncodeFloat(y, uint.MaxValue>>1, yMin, yMax);
        }
        else
        {
            Stream.Read(block, 1, 2);
            point.dt = IdleLength;
            point.x = EncodeFloat((uint)block[1]-1, byte.MaxValue - 2, xMin, xMax);
            point.y = EncodeFloat(block[2], byte.MaxValue, yMin, yMax);
            //Debug.Log(Stream.Position.ToString() + " raw small in: " + block[0].ToString() + "," + block[1].ToString());
            point.MouseDown = true;
            point.MouseChange = false;
        }
        return point;
    }
    */
        public Point Next()
        {
            var point = new Point { MouseDown = false, MouseChange = false };

            Stream.Read(block, 0, 1);
            point.dt = block[0];
            if (point.dt == IdleCode)
            {
                point.dt = IdleLength;
                return point;
            }

            Stream.Read(block, 1, BlockSize - 1);
            int idx = 1;
            uint x = System.BitConverter.ToUInt32(block, idx); idx += sizeof(uint);
            uint y = System.BitConverter.ToUInt32(block, idx); idx += sizeof(uint);

            if (point.dt != MouseDragCode)
            {
                point.MouseDown = (y & MouseDown) > 0;
                y = y >> 1;
                point.MouseChange = true;
            }
            else
            {
                point.dt = IdleLength;
                point.MouseDown = true;
                point.MouseChange = false;
            }

            point.x = EncodeFloat(x, uint.MaxValue, xMin, xMax);
            point.y = EncodeFloat(y, uint.MaxValue >> 1, yMin, yMax);

            return point;
        }

        public bool HasReadToEnd
        {
            get { if (Stream == null) return true; return Stream.Position >= Stream.Length; }
        }

        private uint TrimToRange(float v, uint range, int min, int max)
        {
            return (uint)(((v - min) / (max - min)) * range);
        }

        private System.Diagnostics.Stopwatch timer;
        private Point lastPoint;
        public void WriteCurrentState()
        {
            var newPoint = new Point();
            newPoint.MouseChange = Input.GetMouseButton(0) != lastPoint.MouseDown;
            newPoint.MouseDown = Input.GetMouseButton(0);
            newPoint.dt = timer.ElapsedMilliseconds;
            newPoint.x = Input.mousePosition.x;
            newPoint.y = Input.mousePosition.y;

            bool sampling = false;

            if (Input.GetMouseButton(0) != lastPoint.MouseDown)
                sampling = true;
            else if (Input.GetMouseButton(0) && (timer.ElapsedMilliseconds > IdleLength))
                sampling = true;
            else if (timer.ElapsedMilliseconds > IdleLength)
                sampling = true;
            if(sampling)
            {
                timer.Restart();
                Write(newPoint);
                lastPoint = newPoint;
            }
        }
        /*
        public void Write(Point p)
        {
            if (!p.MouseDown && !p.MouseChange)
            {
                Stream.WriteByte(IdleCode);
                return;
            }
            else if (p.MouseDown && !p.MouseChange)
            {
                byte x = (byte)(TrimToRange(p.x, byte.MaxValue-2, xMin, xMax)+1);
                byte y = (byte)TrimToRange(p.y, byte.MaxValue, yMin, yMax);
                //Debug.Log("raw small out: " + x.ToString() + "," + y.ToString());

                //Stream.WriteByte((byte)p.dt);
                Stream.WriteByte(MouseDragCode);
                Stream.WriteByte(x);
                Stream.WriteByte(y);
            }
            else if (p.MouseChange)
            {
                uint _x = TrimToRange(p.x, uint.MaxValue, xMin, xMax);
                uint _y = TrimToRange(p.y, uint.MaxValue>>1, yMin, yMax);
                //Debug.Log("raw big out: " + _x.ToString() + "," + _y.ToString());
                _y = _y << 1;
                if (p.MouseDown)
                    _y |= MouseDown;

                byte[] x = System.BitConverter.GetBytes(_x);
                byte[] y = System.BitConverter.GetBytes(_y);

                Stream.WriteByte((byte)p.dt);
                //Stream.WriteByte(MouseCode);
                Stream.Write(x, 0, sizeof(uint));
                Stream.Write(y, 0, sizeof(uint));
            }
        }
        */
        public void Write(Point p)
        {
            if (!p.MouseDown && !p.MouseChange)
            {
                Stream.WriteByte(IdleCode);
                return;
            }

            uint _x = TrimToRange(p.x, uint.MaxValue, xMin, xMax);
            uint _y = TrimToRange(p.y, uint.MaxValue >> 1, yMin, yMax);
            if (p.MouseDown && !p.MouseChange)
            {
                Stream.WriteByte(MouseDragCode);
            }
            else if (p.MouseChange)
            {
                Stream.WriteByte((byte)p.dt);
                _y = _y << 1;
                if (p.MouseDown)
                    _y |= MouseDown;
            }

            byte[] x = System.BitConverter.GetBytes(_x);
            byte[] y = System.BitConverter.GetBytes(_y);

            Stream.Write(x, 0, sizeof(uint));
            Stream.Write(y, 0, sizeof(uint));
        }

        public void StartRecording()
        {
            Stream = new MemoryStream();

            xMin = 0;
            xMax = Screen.width;
            yMin = 0;
            yMax = Screen.height;

            Stream.Write(System.BitConverter.GetBytes(xMin), 0, sizeof(int));
            Stream.Write(System.BitConverter.GetBytes(xMax), 0, sizeof(int));
            Stream.Write(System.BitConverter.GetBytes(yMin), 0, sizeof(int));
            Stream.Write(System.BitConverter.GetBytes(yMax), 0, sizeof(int));

            timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            lastPoint = new Point { dt = 0, MouseChange = false, MouseDown = Input.GetMouseButton(0), x = 0, y = 0 };
        }
        public void Save(string filename)
        {
            using (var file = File.Open(Path.Combine(Application.persistentDataPath, filename) , FileMode.Create))
            {
                Debug.Log("Saving replay with "+Stream.Length.ToString()+" bytes");
                Stream.WriteTo(file);
                file.Flush();
            }
            Stream = null;
        }

        public void Load(string filename)
        {
            int headerSize = sizeof(int) * 4;
            Stream = new MemoryStream(File.ReadAllBytes(Path.Combine(Application.persistentDataPath, filename)));

            byte[] header = new byte[headerSize];
            Stream.Read(header, 0, headerSize);
            int idx = 0;
            //Debug.Log(idx.ToString());
            xMin = System.BitConverter.ToInt32(header, idx); idx += sizeof(int);
            //Debug.Log(idx.ToString());
            xMax = System.BitConverter.ToInt32(header, idx); idx += sizeof(int);
            //Debug.Log(idx.ToString());
            yMin = System.BitConverter.ToInt32(header, idx); idx += sizeof(int);
            //Debug.Log(idx.ToString());
            yMax = System.BitConverter.ToInt32(header, idx); idx += sizeof(int);
            //Stream.Seek(idx, SeekOrigin.Begin);
            //Debug.Log(idx.ToString()+" = "+Stream.Position.ToString());

            //Debug.Log("x:(" + xMin.ToString() + "," + xMax.ToString() + ") y:(" + yMin.ToString() + "," + yMax.ToString() + ")");
        }
    }
}
