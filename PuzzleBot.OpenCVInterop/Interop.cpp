// Copyright (c) 2016 Andrew Robinson. All rights reserved.

#include <iostream>
#include <thread>
#include <mutex>
#include <memory>
#include <chrono>

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
            : m_capture(captureUrl.c_str())
            , m_captureThread([this] { Capturer(); })
            , m_frameGrabbed(false)
            , m_die(false)
            , m_captureUrl(std::move(captureUrl))
        {}

        // Snaps a frame from the camera, if the last captured
        // frame has been grabbed this method blocks until a new
        // frame is available. If a new frame isn't available within
        // 500ms, this method return false and an empty ptr.
        std::pair<bool, std::unique_ptr<cv::Mat>> TryGrabFrame()
        {
            std::unique_lock<std::mutex> l(m_mutex);
            if (m_newFrame.wait_for(l, std::chrono::milliseconds(500), [this] { return m_frameGrabbed; })) {
                m_frameGrabbed = false;
                return std::make_pair(true, std::make_unique<cv::Mat>(std::move(m_lastFrame)));
            }
            else {
                return { false, std::unique_ptr<cv::Mat>() };
            }
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
                    m_frameGrabbed = true;
                    l.unlock();
                    m_newFrame.notify_one();
                }
                else {
                    // We might have lost connectivity with the webcam... try to
                    // recreate the capturer.
                    std::this_thread::sleep_for(std::chrono::seconds(1));
                    m_capture = cv::VideoCapture(m_captureUrl.c_str());
                }

                if (m_die) return;
            }
        }

        std::string m_captureUrl;
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

EXTERN_DLL_EXPORT bool CaptureEngine_TryGrabFrame(void* handle, void** frame)
{
    auto result = static_cast<PuzzleBot::OpenCvCaptureEngine*>(handle)->TryGrabFrame();
    *frame = result.second.release();
    return result.first;
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
