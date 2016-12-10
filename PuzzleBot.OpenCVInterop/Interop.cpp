// Copyright (c) 2016 Andrew Robinson. All rights reserved.

#include <iostream>
#include <thread>
#include <mutex>
#include <memory>

#include <opencv2\core.hpp>
#include <opencv2\videoio.hpp>

#define EXTERN_DLL_EXPORT extern "C" __declspec(dllexport)

namespace PuzzleBot
{
    // A wrapper around the OpenCV VideoCapture class that
    // runs a background thread responsible for continuously
    // grabbing frames from the live camera.
    class OpenCvCaptureEngine
    {
    public:
        OpenCvCaptureEngine(std::string captureUrl)
            : m_capture(cv::String(captureUrl.c_str()))
            , m_captureThread([this] { Capturer(); })
            , m_frameGrabbed(true)
            , m_die(false)
        {}

        // Snaps a frame from the camera, if the last captured
        // frame has been grabbed this method blocks until a new
        // frame is available.
        std::unique_ptr<cv::Mat> GrabFrame()
        {
            std::unique_lock<std::mutex> l(m_mutex);
            m_newFrame.wait(l, [this] { return !m_frameGrabbed; });
            m_frameGrabbed = true;
            auto ret = std::make_unique<cv::Mat>(std::move(m_lastFrame));
            return ret;
        }

        ~OpenCvCaptureEngine()
        {
            m_die = true;
            m_captureThread.join();
        }

    private:
        // This thread exists to continuously clear the OpenCV VideoCapture
        // buffer and ensure that when we snap frames off the camera we're
        // always capturing as close to realtime as possible.
        void Capturer()
        {
            while (true) {
                cv::Mat frame;
                if (m_capture.read(frame)) {
                    std::unique_lock<std::mutex> l(m_mutex);
                    m_lastFrame = std::move(frame);
                    m_frameGrabbed = false;
                    l.unlock();
                    m_newFrame.notify_one();
                }

                if (m_die) return;
            }
        }

        bool m_die;
        std::mutex m_mutex;
        std::condition_variable m_newFrame;
        cv::Mat m_lastFrame;
        bool m_frameGrabbed;
        cv::VideoCapture m_capture;
        std::thread m_captureThread;
    };
}

// Native interop methods to be PInvoked from C#.

EXTERN_DLL_EXPORT void* CaptureEngine_Create(const char* captureUrl)
{
    return new PuzzleBot::OpenCvCaptureEngine(captureUrl);
}

EXTERN_DLL_EXPORT void CaptureEngine_Destroy(void* handle)
{
    delete static_cast<PuzzleBot::OpenCvCaptureEngine*>(handle);
}

EXTERN_DLL_EXPORT void* CaptureEngine_GrabFrame(void* handle)
{
    auto frame = static_cast<PuzzleBot::OpenCvCaptureEngine*>(handle)->GrabFrame();
    return frame.release();
}

EXTERN_DLL_EXPORT void Mat_Destroy(void* handle)
{
    delete static_cast<cv::Mat*>(handle);
}

EXTERN_DLL_EXPORT int Mat_GetRows(void* handle)
{
    return static_cast<cv::Mat*>(handle)->rows;
}

EXTERN_DLL_EXPORT int Mat_GetColumns(void* handle)
{
    return static_cast<cv::Mat*>(handle)->cols;
}

EXTERN_DLL_EXPORT char* Mat_GetData(void* handle)
{
    return reinterpret_cast<char*>(static_cast<cv::Mat*>(handle)->data);
}
